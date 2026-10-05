namespace Arniston_Letting_API.DTOs.Reports;

public class ReportDto
{
    public int ReportId { get; set; }

    public string ReportType { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public string GeneratedBy { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? Description { get; set; }
}