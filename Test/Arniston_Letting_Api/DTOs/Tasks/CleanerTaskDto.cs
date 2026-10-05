namespace Arniston_Letting_API.DTOs.Tasks;

public class CleanerTaskDto
{
    public int CleanerTaskId { get; set; }

    public int CleanerId { get; set; }

    public string CleanerName { get; set; } = string.Empty;

    public int LocationId { get; set; }

    public string LocationName { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public TimeSpan Time { get; set; }

    public bool Completed { get; set; }

    public string? Notes { get; set; }
}