namespace Arniston_Letting_API.DTOs.Owners;

public class UpdateOwnerDto
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? AlternativeNumber { get; set; }

    public string PreferredContactMethod { get; set; } = string.Empty;

    public string? Notes { get; set; }
}