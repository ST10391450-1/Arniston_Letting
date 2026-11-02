using Arniston_Letting_API.Data;
using Arniston_Letting_API.DTOs.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arniston_Letting_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    #region Controller Setup

    /*
     *      Functionality for managing dashboard information
     *
     *      [ApiController] Enables API-specific behaviour such as request
     *      validation and automatic responses for invalid model states.
     *
     *      [Route] Defines the API route using the controller name.
     *
     *      [Authorize(Roles = "Admin")] Restricts access to authenticated
     *      users who have the Admin role.
     *
     *      ApplicationDbContext provides access to the database tables
     *      used to retrieve dashboard statistics, bookings, cleaning
     *      tasks, breakages and notifications.
     *
     */

    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    #endregion

    #region Get Dashboard

    /*
     *          Functionality for retrieving dashboard statistics
     *
     *      [HttpGet] Allows an HTTP GET request to retrieve dashboard data.
     *
     *      Creates a DashboardDto containing summary statistics
     *      retrieved from the database.
     *
     *      [CountAsync] Counts the total number of booking records,
     *      properties, owners and cleaners.
     *
     *      Counts outstanding cleaner tasks where Completed is false.
     *
     *      Counts unresolved breakages where Resolved is false.
     *
     *      Counts notifications where the status is Pending.
     *
     *      Returns 200 OK with the completed DashboardDto containing
     *      the latest dashboard statistics.
     *
     */

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

    #endregion

    #region Get Upcoming Bookings

    /*
     *          Functionality for retrieving upcoming bookings
     *
     *      [HttpGet("upcoming-bookings")] Allows an HTTP GET request
     *      to retrieve bookings scheduled for today or a future date.
     *
     *      Retrieves booking records where the check-in date is greater
     *      than or equal to the current date.
     *
     *      [AsNoTracking] Improves read performance because the retrieved
     *      records do not need to be tracked for database updates.
     *
     *      [Where] Filters bookings to include only upcoming reservations.
     *
     *      [OrderBy] Sorts bookings by their check-in date, with the
     *      earliest upcoming bookings appearing first.
     *
     *      [Select] Projects the booking records into objects containing
     *      the booking ID, guest name, property details, dates and rate.
     *
     *      Includes the property name when the associated property exists.
     *
     *      Returns 200 OK with the list of upcoming bookings.
     *
     */

    [HttpGet("upcoming-bookings")]
    public async Task<IActionResult> GetUpcomingBookings()
    {
        var bookings = await _context.Bookings
            .AsNoTracking()
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

    #endregion

    #region Get Todays Cleanings

    /*
     *          Functionality for retrieving today's cleaning tasks
     *
     *      [HttpGet("todays-cleanings")] Allows an HTTP GET request
     *      to retrieve cleaning tasks scheduled for the current date.
     *
     *      [DateTime.Today] Retrieves the current date without the time
     *      component for filtering today's scheduled cleaning tasks.
     *
     *      [AsNoTracking] Improves read performance because the retrieved
     *      records do not need to be tracked for database updates.
     *
     *      [Where] Filters cleaning tasks to include only records
     *      scheduled for today's date.
     *
     *      [OrderBy] Sorts cleaning tasks by their scheduled time,
     *      with the earliest tasks appearing first.
     *
     *      [Select] Projects the cleaning task records into objects
     *      containing the task ID, cleaner details, property details,
     *      scheduled date, time, completion status and notes.
     *
     *      Includes the cleaner and property names when their associated
     *      records exist.
     *
     *      Returns 200 OK with the list of today's cleaning tasks.
     *
     */

    [HttpGet("todays-cleanings")]
    public async Task<IActionResult> GetTodaysCleanings()
    {
        var today = DateTime.Today;

        var tasks = await _context.CleanerTasks
            .AsNoTracking()
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

    #endregion

    #region Get Open Breakages

    /*
     *          Functionality for retrieving unresolved property breakages
     *
     *      [HttpGet("open-breakages")] Allows an HTTP GET request
     *      to retrieve all breakages that have not been resolved.
     *
     *      [AsNoTracking] Improves read performance because the retrieved
     *      records do not need to be tracked for database updates.
     *
     *      [Where] Filters breakage records to include only those where
     *      the Resolved status is false.
     *
     *      [OrderByDescending] Sorts breakages by their reported date,
     *      with the most recent records appearing first.
     *
     *      [ThenByDescending] Sorts breakages by their reported time
     *      when multiple records share the same date.
     *
     *      [Select] Projects the breakage records into objects containing
     *      the breakage ID, property details, booking reference,
     *      reported date, time and description.
     *
     *      Includes the property name when the associated property exists.
     *
     *      Returns 200 OK with the list of unresolved breakages.
     *
     */

    [HttpGet("open-breakages")]
    public async Task<IActionResult> GetOpenBreakages()
    {
        var breakages = await _context.Breakages
            .AsNoTracking()
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

    #endregion
}