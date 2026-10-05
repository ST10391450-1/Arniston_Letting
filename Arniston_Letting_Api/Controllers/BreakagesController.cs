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
    private readonly ApplicationDbContext _context;

    public BreakagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BreakageDto>>> GetBreakages()
    {
        var breakages = await _context.Breakages
            .AsNoTracking()
            .Include(b => b.Location)
            .OrderByDescending(b => b.Date)
            .ThenByDescending(b => b.Time)
            .ToListAsync();

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

    [HttpPost]
    public async Task<ActionResult<BreakageDto>> CreateBreakage(
        [FromBody] CreateBreakageDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

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

            if (booking.PropertyId != request.LocationId)
            {
                return BadRequest(new
                {
                    message = "The selected booking does not belong to the selected property."
                });
            }
        }

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

        await _context.Entry(breakage)
            .Reference(b => b.Location)
            .LoadAsync();

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

            if (booking.PropertyId != request.LocationId)
            {
                return BadRequest(new
                {
                    message = "The selected booking does not belong to the selected property."
                });
            }
        }

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