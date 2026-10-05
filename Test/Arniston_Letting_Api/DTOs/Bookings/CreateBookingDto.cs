using System.ComponentModel.DataAnnotations;

namespace Arniston_Letting_API.DTOs.Bookings;

public class CreateBookingDto
{
    [Range(1, int.MaxValue)]
    public int PropertyId { get; set; }

    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string BookerName { get; set; } = string.Empty;

    [Required]
    public DateTime CheckIn { get; set; }

    [Required]
    public DateTime CheckOut { get; set; }

    [Range(0, 100000000)]
    public decimal Rate { get; set; }
}
