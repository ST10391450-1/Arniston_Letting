using Arniston_Letting_API.Data;
using Arniston_Letting_API.DTOs.Bookings;
using Arniston_Letting_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arniston_Letting_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
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
            .AsNoTracking()
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
            .AsNoTracking()
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

        if (request.CheckOut <= request.CheckIn)
        {
            return BadRequest(new
            {
                message = "Check-out must be after check-in."
            });
        }

        var property = await _context.Properties
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PropertyId == request.PropertyId);

        if (property == null)
        {
            return BadRequest(new
            {
                message = "The selected property does not exist."
            });
        }

        var overlappingBooking = await _context.Bookings
            .AsNoTracking()
            .AnyAsync(b =>
                b.PropertyId == request.PropertyId &&
                request.CheckIn < b.CheckOut &&
                request.CheckOut > b.CheckIn);

        if (overlappingBooking)
        {
            return Conflict(new
            {
                message = "The property is already booked for the selected dates."
            });
        }

        var numberOfNights = (request.CheckOut - request.CheckIn).Days;
        var calculatedRate = property.RatePerNight * numberOfNights;

        var booking = new Booking
        {
            PropertyId = request.PropertyId,
            BookerName = request.BookerName.Trim(),
            CheckIn = request.CheckIn,
            CheckOut = request.CheckOut,
            Rate = calculatedRate
        };

        _context.Bookings.Add(booking);

        await _context.SaveChangesAsync();

        var now = DateTime.Now;

        var notification = new Notification
        {
            Type = "Booking",
            Recipient = "Admin",
            Date = now,
            Time = now.TimeOfDay,
            Message = $"New booking received from {booking.BookerName} for {property.PropertyName}. Total: R{calculatedRate:N2}.",
            Status = "Pending"
        };

        _context.Notifications.Add(notification);

        var cleaner = await _context.Cleaners
            .Where(c => c.Available)
            .OrderBy(c => c.CleanerId)
            .FirstOrDefaultAsync();

        if (cleaner != null)
        {
            var cleanerTask = new CleanerTask
            {
                CleanerId = cleaner.CleanerId,
                LocationId = booking.PropertyId,
                Date = booking.CheckOut,
                Time = new TimeSpan(10, 0, 0),
                Completed = false,
                Notes = $"Cleaning required after booking #{booking.BookingId}."
            };

            _context.CleanerTasks.Add(cleanerTask);

            cleaner.Available = false;

            var cleanerNotification = new Notification
            {
                Type = "CleaningAssigned",
                Recipient = cleaner.FullName,
                Date = now,
                Time = now.TimeOfDay,
                Message = $"You have been assigned cleaning for {property.PropertyName} on {booking.CheckOut:dd/MM/yyyy}.",
                Status = "Pending"
            };

            _context.Notifications.Add(cleanerNotification);
        }

        await _context.SaveChangesAsync();

        var result = await _context.Bookings
            .AsNoTracking()
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

        if (request.CheckOut <= request.CheckIn)
        {
            return BadRequest(new
            {
                message = "Check-out must be after check-in."
            });
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

        var property = await _context.Properties
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PropertyId == request.PropertyId);

        if (property == null)
        {
            return BadRequest(new
            {
                message = "The selected property does not exist."
            });
        }

        var overlappingBooking = await _context.Bookings
            .AsNoTracking()
            .AnyAsync(b =>
                b.BookingId != id &&
                b.PropertyId == request.PropertyId &&
                request.CheckIn < b.CheckOut &&
                request.CheckOut > b.CheckIn);

        if (overlappingBooking)
        {
            return Conflict(new
            {
                message = "The property is already booked for the selected dates."
            });
        }

        var numberOfNights = (request.CheckOut - request.CheckIn).Days;
        var calculatedRate = property.RatePerNight * numberOfNights;

        booking.PropertyId = request.PropertyId;
        booking.BookerName = request.BookerName.Trim();
        booking.CheckIn = request.CheckIn;
        booking.CheckOut = request.CheckOut;
        booking.Rate = calculatedRate;

        await _context.SaveChangesAsync();

        var now = DateTime.Now;

        var notification = new Notification
        {
            Type = "BookingUpdated",
            Recipient = "Admin",
            Date = now,
            Time = now.TimeOfDay,
            Message = $"Booking #{booking.BookingId} for {booking.BookerName} has been updated. Total: R{calculatedRate:N2}.",
            Status = "Pending"
        };

        _context.Notifications.Add(notification);

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