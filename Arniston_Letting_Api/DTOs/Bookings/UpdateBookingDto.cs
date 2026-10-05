namespace Arniston_Letting_API.DTOs.Bookings;

public class UpdateBookingDto
{
    public int PropertyId { get; set; }

    public string BookerName { get; set; } = string.Empty;

    public DateTime CheckIn { get; set; }

    public DateTime CheckOut { get; set; }

    public decimal Rate { get; set; }
}