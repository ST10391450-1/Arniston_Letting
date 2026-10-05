namespace Arniston_Letting_Front.Models.Bookings;

public class BookingDto
{
    public int BookingId { get; set; }

    public int PropertyId { get; set; }

    public string PropertyName { get; set; } = string.Empty;

    public string BookerName { get; set; } = string.Empty;

    public DateTime CheckIn { get; set; }

    public DateTime CheckOut { get; set; }

    public decimal Rate { get; set; }
}