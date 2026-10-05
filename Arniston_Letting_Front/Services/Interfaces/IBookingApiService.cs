using Arniston_Letting_Front.Models.Bookings;

namespace Arniston_Letting_Front.Services.Interfaces;

public interface IBookingApiService
{
    Task<IEnumerable<BookingDto>> GetAllAsync();

    Task<BookingDto?> GetByIdAsync(int id);

    Task<BookingDto?> CreateAsync(CreateBookingDto request);

    Task<bool> UpdateAsync(int id, UpdateBookingDto request);

    Task<bool> DeleteAsync(int id);
}