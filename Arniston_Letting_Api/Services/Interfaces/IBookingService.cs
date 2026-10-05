using Arniston_Letting_API.DTOs.Bookings;

namespace Arniston_Letting_API.Services.Interfaces;

public interface IBookingService
{
    Task<IEnumerable<BookingDto>> GetAllAsync();
    Task<BookingDto?> GetByIdAsync(int id);
    Task<BookingDto?> CreateAsync(CreateBookingDto request);
    Task<bool> UpdateAsync(int id, UpdateBookingDto request);
    Task<bool> DeleteAsync(int id);
}