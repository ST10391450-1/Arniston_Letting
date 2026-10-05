namespace Arniston_Letting_Front.Models.Bookings;

public class CreateBookingDto
{
    public int PropertyId { get; set; }

    public string BookerName { get; set; } = string.Empty;

    public DateTime CheckIn { get; set; }

    public DateTime CheckOut { get; set; }

    public decimal Rate { get; set; }
}