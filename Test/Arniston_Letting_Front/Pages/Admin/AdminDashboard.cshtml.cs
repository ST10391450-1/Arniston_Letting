using Arniston_Letting_Front.Models.Bookings;
using Arniston_Letting_Front.Models.Breakages;
using Arniston_Letting_Front.Models.Dashboard;
using Arniston_Letting_Front.Models.Tasks;
using Arniston_Letting_Front.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Arniston_Letting_Front.Pages.Admin;

public class AdminDashboardModel : PageModel
{
    private readonly IDashboardApiService _dashboardApiService;

    public AdminDashboardModel(IDashboardApiService dashboardApiService)
    {
        _dashboardApiService = dashboardApiService;
    }

    public DashboardDto Dashboard { get; set; } = new();

    public IEnumerable<BookingDto> UpcomingBookings { get; set; } = Enumerable.Empty<BookingDto>();

    public IEnumerable<CleanerTaskDto> TodaysCleanings { get; set; } = Enumerable.Empty<CleanerTaskDto>();

    public IEnumerable<BreakageDto> OpenBreakages { get; set; } = Enumerable.Empty<BreakageDto>();

    public bool ApiAvailable { get; set; } = true;

    public async Task OnGetAsync()
    {
        var dashboard = await _dashboardApiService.GetDashboardAsync();

        if (dashboard == null)
        {
            ApiAvailable = false;
            return;
        }

        Dashboard = dashboard;
        UpcomingBookings = (await _dashboardApiService.GetUpcomingBookingsAsync()).Take(5);
        TodaysCleanings = await _dashboardApiService.GetTodaysCleaningsAsync();
        OpenBreakages = await _dashboardApiService.GetOpenBreakagesAsync();
    }
}
