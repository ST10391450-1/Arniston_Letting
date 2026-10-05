using System.ComponentModel.DataAnnotations;

namespace Arniston_Letting_API.DTOs.Notifications;

public class UpdateNotificationDto
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Recipient { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public TimeSpan Time { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;

    [Required]
    [StringLength(30, MinimumLength = 2)]
    public string Status { get; set; } = string.Empty;
}