using System.ComponentModel.DataAnnotations;

namespace Arniston_Letting_API.DTOs.Cleaners;

public class UpdateCleanerDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    public bool Available { get; set; }
}