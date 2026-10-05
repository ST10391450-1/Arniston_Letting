namespace Arniston_Letting_API.DTOs.Properties;

public class PropertyDto
{
    public int PropertyId { get; set; }

    public string PropertyName { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public bool Occupied { get; set; }

    public DateTime? OccupiedUntil { get; set; }

    public int OwnerId { get; set; }

    public string OwnerName { get; set; } = string.Empty;

    public int Bedrooms { get; set; }

    public int Sleeps { get; set; }

    public decimal RatePerNight { get; set; }

    public string? Description { get; set; }

    public bool Parking { get; set; }

    public bool Pool { get; set; }
}