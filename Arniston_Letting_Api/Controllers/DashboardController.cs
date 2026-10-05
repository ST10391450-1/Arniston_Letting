using Arniston_Letting_API.Data;
using Arniston_Letting_API.DTOs.Dashboard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arniston_Letting_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> GetDashboard()
    {
        var dashboard = new DashboardDto
        {
            TotalBookings = await _context.Bookings.CountAsync(),

            TotalProperties = await _context.Properties.CountAsync(),

            TotalOwners = await _context.Owners.CountAsync(),

            TotalCleaners = await _context.Cleaners.CountAsync(),

            PendingTasks = await _context.CleanerTasks
                .CountAsync(t => !t.Completed),

            OpenBreakages = await _context.Breakages
                .CountAsync(b => !b.Resolved),

            PendingNotifications = await _context.Notifications
                .CountAsync(n => n.Status == "Pending")
        };

        return Ok(dashboard);
    }

    [HttpGet("upcoming-bookings")]
    public async Task<IActionResult> GetUpcomingBookings()
    {
        var bookings = await _context.Bookings
            .Include(b => b.Property)
            .Where(b => b.CheckIn >= DateTime.Today)
            .OrderBy(b => b.CheckIn)
            .Select(b => new
            {
                b.BookingId,
                b.BookerName,
                b.PropertyId,
                PropertyName = b.Property != null
                    ? b.Property.PropertyName
                    : string.Empty,
                b.CheckIn,
                b.CheckOut,
                b.Rate
            })
            .ToListAsync();

        return Ok(bookings);
    }

    [HttpGet("todays-cleanings")]
    public async Task<IActionResult> GetTodaysCleanings()
    {
        var today = DateTime.Today;

        var tasks = await _context.CleanerTasks
            .Include(t => t.Cleaner)
            .Include(t => t.Location)
            .Where(t => t.Date.Date == today)
            .OrderBy(t => t.Time)
            .Select(t => new
            {
                t.CleanerTaskId,
                t.CleanerId,
                CleanerName = t.Cleaner != null
                    ? t.Cleaner.FullName
                    : string.Empty,
                t.LocationId,
                LocationName = t.Location != null
                    ? t.Location.PropertyName
                    : string.Empty,
                t.Date,
                t.Time,
                t.Completed,
                t.Notes
            })
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpGet("open-breakages")]
    public async Task<IActionResult> GetOpenBreakages()
    {
        var breakages = await _context.Breakages
            .Include(b => b.Location)
            .Where(b => !b.Resolved)
            .OrderByDescending(b => b.Date)
            .ThenByDescending(b => b.Time)
            .Select(b => new
            {
                b.BreakageId,
                b.LocationId,
                LocationName = b.Location != null
                    ? b.Location.PropertyName
                    : string.Empty,
                b.BookingId,
                b.Date,
                b.Time,
                b.ReportedBy,
                b.Notes,
                b.Resolved
            })
            .ToListAsync();

        return Ok(breakages);
    }
}