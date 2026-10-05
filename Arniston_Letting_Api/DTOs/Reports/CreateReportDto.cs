using System.ComponentModel.DataAnnotations;

namespace Arniston_Letting_API.DTOs.Reports;

public class CreateReportDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string ReportType { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string GeneratedBy { get; set; } = string.Empty;

    [Required]
    [StringLength(30, MinimumLength = 2)]
    public string Status { get; set; } = string.Empty;

    [StringLength(5000)]
    public string? Description { get; set; }
}