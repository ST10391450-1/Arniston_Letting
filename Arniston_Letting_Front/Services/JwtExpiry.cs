using System.Text.Json;

namespace Arniston_Letting_Front.Services;

/// <summary>
/// Reads the "exp" claim of a JWT without validating its signature. The front
/// end only needs to know WHEN the API will stop accepting the token; the API
/// remains the sole authority on whether a token is genuine.
/// </summary>
public static class JwtExpiry
{
    // Treat tokens that are about to expire as expired, so a request is never
    // sent with a token that will lapse in flight.
    private static readonly TimeSpan Skew = TimeSpan.FromSeconds(30);

    public static bool IsExpired(string? jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
        {
            return true;
        }

        try
        {
            var parts = jwt.Split('.');

            if (parts.Length < 2)
            {
                return true;
            }

            // JWT segments are base64url encoded without padding.
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload += new string('=', (4 - payload.Length % 4) % 4);

            using var document = JsonDocument.Parse(
                Convert.FromBase64String(payload));

            if (!document.RootElement.TryGetProperty("exp", out var exp) ||
                !exp.TryGetInt64(out var seconds))
            {
                return true;
            }

            return DateTimeOffset.FromUnixTimeSeconds(seconds) <=
                   DateTimeOffset.UtcNow.Add(Skew);
        }
        catch (Exception)
        {
            // Anything malformed is treated as unusable.
            return true;
        }
    }
}
