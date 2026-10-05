using Arniston_Letting_API.Data;
using Arniston_Letting_API.DTOs.Auth;
using Arniston_Letting_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Arniston_Letting_API.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        if (request == null)
        {
            return null;
        }

        var email = NormalizeEmail(request.Email);

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Email == email &&
                u.IsActive);

        if (user == null)
        {
            return null;
        }

        bool passwordValid;

        try
        {
            passwordValid = BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash);
        }
        catch
        {
            return null;
        }

        if (!passwordValid)
        {
            return null;
        }

        var token = GenerateToken(user);

        return new LoginResponse
        {
            Success = true,
            Message = "Login successful.",
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            Token = token
        };
    }

    public async Task<LoginResponse?> RegisterAsync(
        RegisterRequest request)
    {
        if (request == null)
        {
            return null;
        }

        var email = NormalizeEmail(request.Email);

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return null;
        }

        var existingUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (existingUser != null)
        {
            return null;
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(
            request.Password,
            BCrypt.Net.BCrypt.GenerateSalt(12));

        var user = new Models.User
        {
            Email = email,
            PasswordHash = passwordHash,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = "User",
            IsActive = true
        };

        _context.Users.Add(user);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return null;
        }

        var token = GenerateToken(user);

        return new LoginResponse
        {
            Success = true,
            Message = "Account created successfully.",
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            Token = token
        };
    }

    public async Task<bool> LogoutAsync(int userId)
    {
        if (userId <= 0)
        {
            return false;
        }

        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.UserId == userId);

        return userExists;
    }

    private string GenerateToken(Models.User user)
    {
        var key = _configuration["Jwt:Key"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "JWT key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new InvalidOperationException(
                "JWT issuer is not configured.");
        }

        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException(
                "JWT audience is not configured.");
        }

        if (Encoding.UTF8.GetByteCount(key) < 32)
        {
            throw new InvalidOperationException(
                "JWT key must be at least 32 bytes long.");
        }

        var expiresInMinutes = _configuration.GetValue<int>(
            "Jwt:ExpiresInMinutes",
            60);

        if (expiresInMinutes <= 0)
        {
            expiresInMinutes = 60;
        }

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.UserId.ToString()),

            new Claim(
                ClaimTypes.Email,
                user.Email),

            new Claim(
                ClaimTypes.Name,
                $"{user.FirstName} {user.LastName}"),

            new Claim(
                ClaimTypes.Role,
                user.Role)
        };

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    private static string NormalizeEmail(string? email)
    {
        return email?.Trim().ToLowerInvariant() ?? string.Empty;
    }
}