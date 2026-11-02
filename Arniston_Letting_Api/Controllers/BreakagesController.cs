using Arniston_Letting_API.Data;
using Arniston_Letting_API.DTOs.Breakages;
using Arniston_Letting_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arniston_Letting_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class BreakagesController : ControllerBase
{
    #region Controller Setup

    /*
     *      Functionality for managing reported property breakages
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
     *      used to manage breakages, properties and bookings.
     *
     */

    private readonly ApplicationDbContext _context;

    public BreakagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    #endregion

    #region Get Breakages

    /*
     *          Functionality for retrieving all reported breakages
     *
     *      [HttpGet] Allows an HTTP GET request to retrieve breakages.
     *
     *      Retrieves all breakage records from the database and includes
     *      the associated property information.
     *
     *      [AsNoTracking] Improves read performance because the retrieved
     *      records do not need to be tracked for database updates.
     *
     *      [Include] Loads the associated property information for each
     *      breakage record.
     *
     *      [OrderByDescending] Sorts breakages by their reported date,
     *      with the most recent records appearing first.
     *
     *      [ThenByDescending] Sorts breakages by their reported time
     *      when multiple records share the same date.
     *
     *      [Select] Converts the retrieved breakage records into
     *      BreakageDto objects containing the relevant details.
     *
     *      Returns 200 OK with the list of reported breakages.
     *
     */

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BreakageDto>>> GetBreakages()
    {
        var breakages = await _context.Breakages
            .AsNoTracking()
            .Include(b => b.Location)
            .OrderByDescending(b => b.Date)
            .ThenByDescending(b => b.Time)
            .ToListAsync();

        // Convert the retrieved breakages into DTO objects.
        var result = breakages.Select(b => new BreakageDto
        {
            BreakageId = b.BreakageId,
            LocationId = b.LocationId,
            LocationName = b.Location?.PropertyName ?? string.Empty,
            BookingId = b.BookingId,
            Date = b.Date,
            Time = b.Time,
            ReportedBy = b.ReportedBy,
            Notes = b.Notes,
            Resolved = b.Resolved
        });

        return Ok(result);
    }

    /*
     *          Functionality for retrieving a single reported breakage
     *
     *      [HttpGet("{id:int}")] Retrieves a breakage using its ID.
     *
     *      Searches the database for the requested breakage and includes
     *      the associated property information.
     *
     *      [AsNoTracking] Improves read performance because the retrieved
     *      record does not need to be tracked for database updates.
     *
     *      [FirstOrDefaultAsync] Retrieves the first matching breakage
     *      or returns null if the record does not exist.
     *
     *      Converts the retrieved record into a BreakageDto containing
     *      the property name, booking ID, date, time and report details.
     *
     *      If the breakage does not exist, returns 404 Not Found.
     *
     *      If the breakage exists, returns 200 OK with its details.
     *
     */

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BreakageDto>> GetBreakage(int id)
    {
        var breakage = await _context.Breakages
            .AsNoTracking()
            .Include(b => b.Location)
            .FirstOrDefaultAsync(b => b.BreakageId == id);

        if (breakage == null)
        {
            return NotFound(new
            {
                message = "Breakage not found."
            });
        }

        // Convert the retrieved breakage into a DTO object.
        var result = new BreakageDto
        {
            BreakageId = breakage.BreakageId,
            LocationId = breakage.LocationId,
            LocationName = breakage.Location?.PropertyName ?? string.Empty,
            BookingId = breakage.BookingId,
            Date = breakage.Date,
            Time = breakage.Time,
            ReportedBy = breakage.ReportedBy,
            Notes = breakage.Notes,
            Resolved = breakage.Resolved
        };

        return Ok(result);
    }

    #endregion

    #region Create Breakage

    /*
     *          Functionality for reporting a new property breakage
     *
     *      [HttpPost] Allows an HTTP POST request to create a breakage.
     *
     *      Validates the incoming request and checks that the selected
     *      property exists in the database.
     *
     *      If a BookingId is provided, verifies that the booking exists
     *      and belongs to the selected property.
     *
     *      Creates a new Breakage record using the validated request
     *      details and removes unnecessary whitespace from the text fields.
     *
     *      Saves the new breakage record to the database.
     *
     *      [Reference.LoadAsync] Loads the associated property information
     *      so that the property name can be included in the response.
     *
     *      Converts the created record into a BreakageDto.
     *
     *      Returns 400 Bad Request for invalid request data, a missing
     *      property or an invalid booking reference.
     *
     *      Returns 201 Created with the newly created breakage and its
     *      location when the operation succeeds.
     *
     */

    [HttpPost]
    public async Task<ActionResult<BreakageDto>> CreateBreakage(
        [FromBody] CreateBreakageDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verify that the selected property exists.
        var locationExists = await _context.Properties
            .AsNoTracking()
            .AnyAsync(p => p.PropertyId == request.LocationId);

        if (!locationExists)
        {
            return BadRequest(new
            {
                message = "The selected property does not exist."
            });
        }

        if (request.BookingId.HasValue)
        {
            // Verify that the selected booking exists.
            var booking = await _context.Bookings
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    b => b.BookingId == request.BookingId.Value);

            if (booking == null)
            {
                return BadRequest(new
                {
                    message = "The selected booking does not exist."
                });
            }

            // Ensure that the booking belongs to the selected property.
            if (booking.PropertyId != request.LocationId)
            {
                return BadRequest(new
                {
                    message = "The selected booking does not belong to the selected property."
                });
            }
        }

        // Create the breakage record using the validated request details.
        var breakage = new Breakage
        {
            LocationId = request.LocationId,
            BookingId = request.BookingId,
            Date = request.Date,
            Time = request.Time,
            ReportedBy = request.ReportedBy?.Trim(),
            Notes = request.Notes?.Trim(),
            Resolved = request.Resolved
        };

        _context.Breakages.Add(breakage);

        await _context.SaveChangesAsync();

        // Load the associated property information for the response.
        await _context.Entry(breakage)
            .Reference(b => b.Location)
            .LoadAsync();

        // Convert the created breakage into a DTO object.
        var result = new BreakageDto
        {
            BreakageId = breakage.BreakageId,
            LocationId = breakage.LocationId,
            LocationName = breakage.Location?.PropertyName ?? string.Empty,
            BookingId = breakage.BookingId,
            Date = breakage.Date,
            Time = breakage.Time,
            ReportedBy = breakage.ReportedBy,
            Notes = breakage.Notes,
            Resolved = breakage.Resolved
        };

        return CreatedAtAction(
            nameof(GetBreakage),
            new { id = breakage.BreakageId },
            result);
    }

    #endregion

    #region Update Breakage

    /*
     *          Functionality for updating an existing reported breakage
     *
     *      [HttpPut("{id:int}")] Allows an HTTP PUT request to update
     *      the breakage identified by its ID.
     *
     *      Validates the incoming request and checks that the specified
     *      breakage exists in the database.
     *
     *      Verifies that the selected property exists.
     *
     *      If a BookingId is provided, verifies that the booking exists
     *      and belongs to the selected property.
     *
     *      Updates the existing breakage with the validated request details,
     *      including its property, booking reference, date, time,
     *      reported by information, notes and resolution status.
     *
     *      Saves the updated breakage record to the database.
     *
     *      Returns 400 Bad Request for invalid request data, a missing
     *      property or an invalid booking reference.
     *
     *      Returns 404 Not Found if the breakage does not exist.
     *
     *      Returns 204 No Content when the update succeeds.
     *
     */

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateBreakage(
        int id,
        [FromBody] UpdateBreakageDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Find the breakage that needs to be updated.
        var breakage = await _context.Breakages
            .FirstOrDefaultAsync(b => b.BreakageId == id);

        if (breakage == null)
        {
            return NotFound(new
            {
                message = "Breakage not found."
            });
        }

        // Verify that the selected property exists.
        var locationExists = await _context.Properties
            .AsNoTracking()
            .AnyAsync(p => p.PropertyId == request.LocationId);

        if (!locationExists)
        {
            return BadRequest(new
            {
                message = "The selected property does not exist."
            });
        }

        if (request.BookingId.HasValue)
        {
            // Verify that the selected booking exists.
            var booking = await _context.Bookings
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    b => b.BookingId == request.BookingId.Value);

            if (booking == null)
            {
                return BadRequest(new
                {
                    message = "The selected booking does not exist."
                });
            }

            // Ensure that the booking belongs to the selected property.
            if (booking.PropertyId != request.LocationId)
            {
                return BadRequest(new
                {
                    message = "The selected booking does not belong to the selected property."
                });
            }
        }

        // Update the existing breakage with the validated request details.
        breakage.LocationId = request.LocationId;
        breakage.BookingId = request.BookingId;
        breakage.Date = request.Date;
        breakage.Time = request.Time;
        breakage.ReportedBy = request.ReportedBy?.Trim();
        breakage.Notes = request.Notes?.Trim();
        breakage.Resolved = request.Resolved;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    #endregion

    #region Delete Breakage

    /*
     *          Functionality for deleting an existing reported breakage
     *
     *      [HttpDelete("{id:int}")] Allows an HTTP DELETE request to
     *      delete the breakage identified by its ID.
     *
     *      Searches the database for the specified breakage.
     *
     *      If the breakage does not exist, returns 404 Not Found.
     *
     *      If the breakage exists, removes the record from the database
     *      and saves the changes.
     *
     *      Returns 204 No Content when the deletion succeeds.
     *
     */

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBreakage(int id)
    {
        // Find the breakage that needs to be deleted.
        var breakage = await _context.Breakages
            .FirstOrDefaultAsync(b => b.BreakageId == id);

        if (breakage == null)
        {
            return NotFound(new
            {
                message = "Breakage not found."
            });
        }

        // Remove the breakage record from the database.
        _context.Breakages.Remove(breakage);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    #endregion
}