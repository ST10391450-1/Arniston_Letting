using System.ComponentModel.DataAnnotations;

namespace Arniston_Letting_API.DTOs.Owners;

public class UpdateOwnerDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Phone]
    [StringLength(30)]
    public string? AlternativeNumber { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string PreferredContactMethod { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Notes { get; set; }
}