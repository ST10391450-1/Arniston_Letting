using System.Net.Http.Json;
using Arniston_Letting_Front.Models.Bookings;
using Arniston_Letting_Front.Models.Breakages;
using Arniston_Letting_Front.Models.Dashboard;
using Arniston_Letting_Front.Models.Tasks;
using Arniston_Letting_Front.Services.Interfaces;

namespace Arniston_Letting_Front.Services;

public class DashboardApiService : IDashboardApiService
{
    private readonly HttpClient _http;

    public DashboardApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<DashboardDto?> GetDashboardAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<DashboardDto>("api/Dashboard");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public Task<IEnumerable<BookingDto>> GetUpcomingBookingsAsync()
        => GetListAsync<BookingDto>("api/Dashboard/upcoming-bookings");

    public Task<IEnumerable<CleanerTaskDto>> GetTodaysCleaningsAsync()
        => GetListAsync<CleanerTaskDto>("api/Dashboard/todays-cleanings");

    public Task<IEnumerable<BreakageDto>> GetOpenBreakagesAsync()
        => GetListAsync<BreakageDto>("api/Dashboard/open-breakages");

    private async Task<IEnumerable<T>> GetListAsync<T>(string url)
    {
        try
        {
            return await _http.GetFromJsonAsync<IEnumerable<T>>(url)
                   ?? Enumerable.Empty<T>();
        }
        catch (HttpRequestException)
        {
            return Enumerable.Empty<T>();
        }
    }
}
