using System.Net.Http.Json;
using Arniston_Letting_Front.Models.Bookings;
using Arniston_Letting_Front.Services.Interfaces;

namespace Arniston_Letting_Front.Services;

public class BookingApiService
    : ApiServiceBase<BookingDto, CreateBookingDto, UpdateBookingDto>, IBookingApiService
{
    public BookingApiService(HttpClient http) : base(http, "api/Bookings")
    {
    }
}
