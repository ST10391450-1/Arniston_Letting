using Arniston_Letting_Front.Models.Bookings;
using Arniston_Letting_Front.Models.Breakages;
using Arniston_Letting_Front.Models.Dashboard;
using Arniston_Letting_Front.Models.Tasks;

namespace Arniston_Letting_Front.Services.Interfaces;

public interface IDashboardApiService
{
    Task<DashboardDto?> GetDashboardAsync();

    Task<IEnumerable<BookingDto>> GetUpcomingBookingsAsync();

    Task<IEnumerable<CleanerTaskDto>> GetTodaysCleaningsAsync();

    Task<IEnumerable<BreakageDto>> GetOpenBreakagesAsync();
}
