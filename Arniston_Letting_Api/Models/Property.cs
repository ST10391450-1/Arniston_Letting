namespace Arniston_Letting_API.Models;

public class Property
{
    public int PropertyId { get; set; }

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

    public Owner? Owner { get; set; }

    public ICollection<Booking> Bookings { get; set; }
        = new List<Booking>();

    public ICollection<CleanerTask> CleanerTasks { get; set; }
        = new List<CleanerTask>();
}