namespace Arniston_Letting_API.DTOs.Tasks;

public class UpdateCleanerTaskDto
{
    public int CleanerId { get; set; }

    public int LocationId { get; set; }

    public DateTime Date { get; set; }

    public TimeSpan Time { get; set; }

    public bool Completed { get; set; }

    public string? Notes { get; set; }
}