using Arniston_Letting_API.Data;
using Arniston_Letting_API.DTOs.Bookings;
using Arniston_Letting_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arniston_Letting_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BookingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookings()
    {
        var bookings = await _context.Bookings
            .Include(b => b.Property)
            .Select(b => new BookingDto
            {
                BookingId = b.BookingId,
                PropertyId = b.PropertyId,
                PropertyName = b.Property != null
                    ? b.Property.PropertyName
                    : string.Empty,
                BookerName = b.BookerName,
                CheckIn = b.CheckIn,
                CheckOut = b.CheckOut,
                Rate = b.Rate
            })
            .OrderBy(b => b.CheckIn)
            .ToListAsync();

        return Ok(bookings);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingDto>> GetBooking(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.Property)
            .Where(b => b.BookingId == id)
            .Select(b => new BookingDto
            {
                BookingId = b.BookingId,
                PropertyId = b.PropertyId,
                PropertyName = b.Property != null
                    ? b.Property.PropertyName
                    : string.Empty,
                BookerName = b.BookerName,
                CheckIn = b.CheckIn,
                CheckOut = b.CheckOut,
                Rate = b.Rate
            })
            .FirstOrDefaultAsync();

        if (booking == null)
        {
            return NotFound(new
            {
                message = "Booking not found."
            });
        }

        return Ok(booking);
    }

    [HttpPost]
    public async Task<ActionResult<BookingDto>> CreateBooking(
        [FromBody] CreateBookingDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var propertyExists = await _context.Properties
            .AnyAsync(p => p.PropertyId == request.PropertyId);

        if (!propertyExists)
        {
            return BadRequest(new
            {
                message = "The selected property does not exist."
            });
        }

        if (request.CheckOut <= request.CheckIn)
        {
            return BadRequest(new
            {
                message = "Check-out must be after check-in."
            });
        }

        var booking = new Booking
        {
            PropertyId = request.PropertyId,
            BookerName = request.BookerName,
            CheckIn = request.CheckIn,
            CheckOut = request.CheckOut,
            Rate = request.Rate
        };

        _context.Bookings.Add(booking);

        await _context.SaveChangesAsync();

        var result = await _context.Bookings
            .Include(b => b.Property)
            .Where(b => b.BookingId == booking.BookingId)
            .Select(b => new BookingDto
            {
                BookingId = b.BookingId,
                PropertyId = b.PropertyId,
                PropertyName = b.Property != null
                    ? b.Property.PropertyName
                    : string.Empty,
                BookerName = b.BookerName,
                CheckIn = b.CheckIn,
                CheckOut = b.CheckOut,
                Rate = b.Rate
            })
            .FirstAsync();

        return CreatedAtAction(
            nameof(GetBooking),
            new { id = booking.BookingId },
            result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateBooking(
        int id,
        [FromBody] UpdateBookingDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking == null)
        {
            return NotFound(new
            {
                message = "Booking not found."
            });
        }

        var propertyExists = await _context.Properties
            .AnyAsync(p => p.PropertyId == request.PropertyId);

        if (!propertyExists)
        {
            return BadRequest(new
            {
                message = "The selected property does not exist."
            });
        }

        if (request.CheckOut <= request.CheckIn)
        {
            return BadRequest(new
            {
                message = "Check-out must be after check-in."
            });
        }

        booking.PropertyId = request.PropertyId;
        booking.BookerName = request.BookerName;
        booking.CheckIn = request.CheckIn;
        booking.CheckOut = request.CheckOut;
        booking.Rate = request.Rate;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBooking(int id)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking == null)
        {
            return NotFound(new
            {
                message = "Booking not found."
            });
        }

        _context.Bookings.Remove(booking);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}