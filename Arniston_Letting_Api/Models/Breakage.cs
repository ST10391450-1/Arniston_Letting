namespace Arniston_Letting_API.Models;

public class Breakage
{
    public int BreakageId { get; set; }

    public int LocationId { get; set; }

    public int? BookingId { get; set; }

    public DateTime Date { get; set; }

    public TimeSpan Time { get; set; }

    public string ReportedBy { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public bool Resolved { get; set; }

    public Property? Location { get; set; }

    public Booking? Booking { get; set; }
}