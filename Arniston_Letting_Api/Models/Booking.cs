namespace Arniston_Letting_API.Models;

public class Booking
{
    public int BookingId { get; set; }

    public int PropertyId { get; set; }

    public string BookerName { get; set; } = string.Empty;

    public DateTime CheckIn { get; set; }

    public DateTime CheckOut { get; set; }

    public decimal Rate { get; set; }

    public Property? Property { get; set; }

    public ICollection<Breakage> Breakages { get; set; }
        = new List<Breakage>();
}