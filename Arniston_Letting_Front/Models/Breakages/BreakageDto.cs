namespace Arniston_Letting_Front.Models.Breakages;

public class BreakageDto
{
    public int BreakageId { get; set; }

    public int LocationId { get; set; }

    public string LocationName { get; set; } = string.Empty;

    public int? BookingId { get; set; }

    public DateTime Date { get; set; }

    public TimeSpan Time { get; set; }

    public string ReportedBy { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public bool Resolved { get; set; }
}