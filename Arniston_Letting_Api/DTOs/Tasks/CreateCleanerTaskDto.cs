using System.ComponentModel.DataAnnotations;

namespace Arniston_Letting_API.DTOs.Tasks;

public class CreateCleanerTaskDto
{
    [Range(1, int.MaxValue)]
    public int CleanerId { get; set; }

    [Range(1, int.MaxValue)]
    public int LocationId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public TimeSpan Time { get; set; }

    public bool Completed { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }
}