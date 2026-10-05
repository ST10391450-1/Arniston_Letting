using System.ComponentModel.DataAnnotations;

namespace Arniston_Letting_API.DTOs.Properties;

public class CreatePropertyDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string PropertyName { get; set; } = string.Empty;

    [Required]
    [StringLength(500, MinimumLength = 5)]
    public string Address { get; set; } = string.Empty;

    public bool Occupied { get; set; }

    public DateTime? OccupiedUntil { get; set; }

    [Range(1, int.MaxValue)]
    public int OwnerId { get; set; }

    [Range(1, 100)]
    public int Bedrooms { get; set; }

    [Range(1, 1000)]
    public int Sleeps { get; set; }

    [Range(0, 100000000)]
    public decimal RatePerNight { get; set; }

    [StringLength(5000)]
    public string? Description { get; set; }

    public bool Parking { get; set; }

    public bool Pool { get; set; }
}