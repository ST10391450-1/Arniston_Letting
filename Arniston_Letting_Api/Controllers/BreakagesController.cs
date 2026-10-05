using Arniston_Letting_API.Data;
using Arniston_Letting_API.DTOs.Breakages;
using Arniston_Letting_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arniston_Letting_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BreakagesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BreakagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BreakageDto>>> GetBreakages()
    {
        var breakages = await _context.Breakages
            .Include(b => b.Location)
            .OrderByDescending(b => b.Date)
            .ThenByDescending(b => b.Time)
            .Select(b => new BreakageDto
            {
                BreakageId = b.BreakageId,
                LocationId = b.LocationId,
                LocationName = b.Location != null
                    ? b.Location.PropertyName
                    : string.Empty,
                BookingId = b.BookingId,
                Date = b.Date,
                Time = b.Time,
                ReportedBy = b.ReportedBy,
                Notes = b.Notes,
                Resolved = b.Resolved
            })
            .ToListAsync();

        return Ok(breakages);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BreakageDto>> GetBreakage(int id)
    {
        var breakage = await _context.Breakages
            .Include(b => b.Location)
            .Where(b => b.BreakageId == id)
            .Select(b => new BreakageDto
            {
                BreakageId = b.BreakageId,
                LocationId = b.LocationId,
                LocationName = b.Location != null
                    ? b.Location.PropertyName
                    : string.Empty,
                BookingId = b.BookingId,
                Date = b.Date,
                Time = b.Time,
                ReportedBy = b.ReportedBy,
                Notes = b.Notes,
                Resolved = b.Resolved
            })
            .FirstOrDefaultAsync();

        if (breakage == null)
        {
            return NotFound(new
            {
                message = "Breakage not found."
            });
        }

        return Ok(breakage);
    }

    [HttpPost]
    public async Task<ActionResult<BreakageDto>> CreateBreakage(
        [FromBody] CreateBreakageDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var locationExists = await _context.Properties
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
            var bookingExists = await _context.Bookings
                .AnyAsync(b => b.BookingId == request.BookingId.Value);

            if (!bookingExists)
            {
                return BadRequest(new
                {
                    message = "The selected booking does not exist."
                });
            }
        }

        var breakage = new Breakage
        {
            LocationId = request.LocationId,
            BookingId = request.BookingId,
            Date = request.Date,
            Time = request.Time,
            ReportedBy = request.ReportedBy,
            Notes = request.Notes,
            Resolved = request.Resolved
        };

        _context.Breakages.Add(breakage);

        await _context.SaveChangesAsync();

        var result = await _context.Breakages
            .Include(b => b.Location)
            .Where(b => b.BreakageId == breakage.BreakageId)
            .Select(b => new BreakageDto
            {
                BreakageId = b.BreakageId,
                LocationId = b.LocationId,
                LocationName = b.Location != null
                    ? b.Location.PropertyName
                    : string.Empty,
                BookingId = b.BookingId,
                Date = b.Date,
                Time = b.Time,
                ReportedBy = b.ReportedBy,
                Notes = b.Notes,
                Resolved = b.Resolved
            })
            .FirstAsync();

        return CreatedAtAction(
            nameof(GetBreakage),
            new { id = breakage.BreakageId },
            result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateBreakage(
        int id,
        [FromBody] UpdateBreakageDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var breakage = await _context.Breakages
            .FirstOrDefaultAsync(b => b.BreakageId == id);

        if (breakage == null)
        {
            return NotFound(new
            {
                message = "Breakage not found."
            });
        }

        var locationExists = await _context.Properties
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
            var bookingExists = await _context.Bookings
                .AnyAsync(b => b.BookingId == request.BookingId.Value);

            if (!bookingExists)
            {
                return BadRequest(new
                {
                    message = "The selected booking does not exist."
                });
            }
        }

        breakage.LocationId = request.LocationId;
        breakage.BookingId = request.BookingId;
        breakage.Date = request.Date;
        breakage.Time = request.Time;
        breakage.ReportedBy = request.ReportedBy;
        breakage.Notes = request.Notes;
        breakage.Resolved = request.Resolved;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBreakage(int id)
    {
        var breakage = await _context.Breakages
            .FirstOrDefaultAsync(b => b.BreakageId == id);

        if (breakage == null)
        {
            return NotFound(new
            {
                message = "Breakage not found."
            });
        }

        _context.Breakages.Remove(breakage);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}