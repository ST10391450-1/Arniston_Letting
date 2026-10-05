using System.ComponentModel.DataAnnotations;

namespace Arniston_Letting_API.DTOs.Breakages;

public class UpdateBreakageDto
{
    [Range(1, int.MaxValue)]
    public int LocationId { get; set; }

    [Range(1, int.MaxValue)]
    public int? BookingId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public TimeSpan Time { get; set; }

    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string ReportedBy { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Notes { get; set; }

    public bool Resolved { get; set; }
}