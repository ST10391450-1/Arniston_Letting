namespace Arniston_Letting_API.DTOs.Breakages;

public class CreateBreakageDto
{
    public int LocationId { get; set; }

    public int? BookingId { get; set; }

    public DateTime Date { get; set; }

    public TimeSpan Time { get; set; }

    public string ReportedBy { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public bool Resolved { get; set; }
}