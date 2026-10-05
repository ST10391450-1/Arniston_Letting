namespace Arniston_Letting_API.DTOs.Properties;

public class CreatePropertyDto
{
    public string PropertyName { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public bool Occupied { get; set; }

    public DateTime? OccupiedUntil { get; set; }

    public int OwnerId { get; set; }

    public int Bedrooms { get; set; }

    public int Sleeps { get; set; }

    public decimal RatePerNight { get; set; }

    public string? Description { get; set; }

    public bool Parking { get; set; }

    public bool Pool { get; set; }
}