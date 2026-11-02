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
    #region Controller Setup

    /*
     *      Functionality for managing bookings
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
     *      used to manage bookings, properties, cleaners and notifications.
     *
     */

    private readonly ApplicationDbContext _context;

    public BookingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    #endregion

    #region Get Bookings

    /*
     *          Functionality for retrieving all bookings
     *
     *      [HttpGet] Allows an HTTP GET request to retrieve bookings.
     *
     *      Retrieves booking records from the database and includes the
     *      associated property information.
     *
     *      [AsNoTracking] Improves read performance because the retrieved
     *      records do not need to be tracked for database updates.
     *
     *      [Select] Converts booking records into BookingDto objects,
     *      including the property name, guest name, dates and rate.
     *
     *      [OrderBy] Sorts bookings by their check-in date.
     *
     *      Returns 200 OK with the list of bookings.
     *
     */

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

    /*
     *          Functionality for retrieving a single booking
     *
     *      [HttpGet("{id:int}")] Retrieves a booking using its ID.
     *
     *      Searches the database for the requested booking and converts
     *      the result into a BookingDto containing the relevant details.
     *
     *      If the booking does not exist, returns 404 Not Found.
     *
     *      If the booking exists, returns 200 OK with the booking details.
     *
     */

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

    #endregion

    #region Create Booking

    /*
     *          Functionality for creating a new booking
     *
     *      [HttpPost] Allows an HTTP POST request to create a booking.
     *
     *      Validates the request and checks that the check-out date is
     *      after the check-in date.
     *
     *      Verifies that the selected property exists and checks for
     *      overlapping bookings to prevent double-booking.
     *
     *      Calculates the total booking rate using the property's
     *      nightly rate multiplied by the number of nights.
     *
     *      Saves the new booking and creates an administrator notification.
     *
     *      If an available cleaner exists, assigns a cleaning task for
     *      the check-out date, marks the cleaner as unavailable and
     *      creates a notification for the assigned cleaner.
     *
     *      Returns 400 Bad Request for invalid data or a missing property,
     *      409 Conflict if the property is already booked, or
     *      201 Created with the newly created booking.
     *
     */

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

        // Check whether another booking overlaps the requested dates.
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

        // Calculate the total price based on the number of nights.
        var numberOfNights = (request.CheckOut - request.CheckIn).Days;
        var calculatedRate = property.RatePerNight * numberOfNights;

        // Create the booking record using the validated request details.
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

        // Create a notification to inform the administrator of the booking.
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

        // Find the first available cleaner and assign the cleaning task.
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

            // Mark the cleaner as unavailable after assigning the task.
            cleaner.Available = false;

            // Notify the cleaner about the assigned cleaning task.
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

        // Save the notifications, cleaning task and cleaner availability.
        await _context.SaveChangesAsync();

        // Retrieve the completed booking with its property name for the response.
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

    #endregion

    #region Update Booking

    /*
     *          Functionality for updating an existing booking
     *
     *      [HttpPut("{id:int}")] Allows an HTTP PUT request to update
     *      the booking identified by its ID.
     *
     *      Validates the request, checks the booking exists, verifies
     *      that the selected property exists and prevents overlapping
     *      bookings for the same property.
     *
     *      Recalculates the total rate using the property's nightly rate
     *      and the updated number of nights.
     *
     *      Saves the updated booking details and creates an administrator
     *      notification describing the change.
     *
     *      Returns 400 Bad Request for invalid details, 404 Not Found
     *      if the booking does not exist, 409 Conflict for overlapping
     *      dates, or 204 No Content when the update succeeds.
     *
     */

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

        // Find the booking that needs to be updated.
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking == null)
        {
            return NotFound(new
            {
                message = "Booking not found."
            });
        }

        // Verify that the requested property exists.
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

        // Check for overlapping bookings while excluding the current booking.
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

        // Recalculate the total price using the updated booking dates.
        var numberOfNights = (request.CheckOut - request.CheckIn).Days;
        var calculatedRate = property.RatePerNight * numberOfNights;

        // Update the existing booking with the validated details.
        booking.PropertyId = request.PropertyId;
        booking.BookerName = request.BookerName.Trim();
        booking.CheckIn = request.CheckIn;
        booking.CheckOut = request.CheckOut;
        booking.Rate = calculatedRate;

        await _context.SaveChangesAsync();

        // Create an administrator notification about the booking update.
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

    #endregion

    #region Delete Booking

    /*
     *          Functionality for deleting an existing booking
     *
     *      [HttpDelete("{id:int}")] Allows an HTTP DELETE request to
     *      delete the booking identified by its ID.
     *
     *      Searches the database for the specified booking.
     *
     *      If the booking does not exist, returns 404 Not Found.
     *
     *      If the booking exists, removes the record and saves the
     *      changes to the database.
     *
     *      Returns 204 No Content when the deletion succeeds.
     *
     */

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBooking(int id)
    {
        // Find the booking that needs to be deleted.
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking == null)
        {
            return NotFound(new
            {
                message = "Booking not found."
            });
        }

        // Remove the booking record from the database.
        _context.Bookings.Remove(booking);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    #endregion
}