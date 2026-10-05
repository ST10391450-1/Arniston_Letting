namespace Arniston_Letting_API.Models;

public class CleanerTask
{
    public int CleanerTaskId { get; set; }

    public int CleanerId { get; set; }

    public int LocationId { get; set; }

    public DateTime Date { get; set; }

    public TimeSpan Time { get; set; }

    public bool Completed { get; set; }

    public string? Notes { get; set; }

    public Cleaner? Cleaner { get; set; }

    public Property? Location { get; set; }
}