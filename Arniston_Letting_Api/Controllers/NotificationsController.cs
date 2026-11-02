using Arniston_Letting_API.Data;
using Arniston_Letting_API.DTOs.Notifications;
using Arniston_Letting_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arniston_Letting_API.Controllers;

/*
 *          Controller Setup
 *
 *      [ApiController] Enables API-specific behaviour, including automatic
 *      model validation and binding of request data.
 *
 *      [Route("api/[controller]")] Sets the base route for all notification
 *      endpoints to api/notifications.
 *
 *      [Authorize(Roles = "Admin")] Restricts access to users with the
 *      Admin role.
 *
 *      ApplicationDbContext Provides access to the application's database
 *      through Entity Framework Core.
 *
 */
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class NotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public NotificationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Get Notifications

    /*
     *          Functionality for retrieving all notifications.
     *
     *      [HttpGet] Allows an HTTP GET request to retrieve all notifications.
     *
     *      Select Projects each notification into a NotificationDto,
     *      returning only the required notification properties.
     *
     *      OrderByDescending Sorts notifications by date, with the newest
     *      dates appearing first.
     *
     *      ThenByDescending Sorts notifications with the same date by time,
     *      placing the latest times first.
     *
     *      ToListAsync Executes the query asynchronously and retrieves
     *      the notification records.
     *
     *      Returns 200 OK with the list of notifications.
     *
     */
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications()
    {
        // Retrieve all notifications and project them into NotificationDto objects.
        var notifications = await _context.Notifications
            .Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                Type = n.Type,
                Recipient = n.Recipient,
                Date = n.Date,
                Time = n.Time,
                Message = n.Message,
                Status = n.Status
            })
            .OrderByDescending(n => n.Date)
            .ThenByDescending(n => n.Time)
            .ToListAsync();

        // Return the retrieved notifications.
        return Ok(notifications);
    }

    #endregion

    #region Get Notification

    /*
     *          Functionality for retrieving a notification by ID.
     *
     *      [HttpGet("{id:int}")] Allows an HTTP GET request to retrieve
     *      a specific notification using its integer ID.
     *
     *      Where Filters the notifications to find the record matching
     *      the supplied notification ID.
     *
     *      Select Projects the matching notification into a NotificationDto.
     *
     *      FirstOrDefaultAsync Executes the query asynchronously and returns
     *      the matching notification, or null if no record exists.
     *
     *      If the notification does not exist, the endpoint returns 404 Not Found
     *      with an explanatory message.
     *
     *      Returns 200 OK with the notification when it is found.
     *
     */
    [HttpGet("{id:int}")]
    public async Task<ActionResult<NotificationDto>> GetNotification(int id)
    {
        // Find the notification matching the supplied ID.
        var notification = await _context.Notifications
            .Where(n => n.NotificationId == id)
            .Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                Type = n.Type,
                Recipient = n.Recipient,
                Date = n.Date,
                Time = n.Time,
                Message = n.Message,
                Status = n.Status
            })
            .FirstOrDefaultAsync();

        // Check whether the requested notification exists.
        if (notification == null)
        {
            return NotFound(new
            {
                message = "Notification not found."
            });
        }

        // Return the notification details.
        return Ok(notification);
    }

    #endregion

    #region Create Notification

    /*
     *          Functionality for creating a new notification.
     *
     *      [HttpPost] Allows an HTTP POST request to create a notification.
     *
     *      CreateNotificationDto Receives the notification details from
     *      the request body.
     *
     *      ModelState.IsValid Checks whether the supplied request data
     *      passes model validation.
     *
     *      Notification Creates a new notification entity using the values
     *      supplied in the request.
     *
     *      Add Adds the new notification entity to the database context.
     *
     *      SaveChangesAsync Saves the new notification record to the database.
     *
     *      NotificationDto Creates the response object containing the saved
     *      notification details, including its generated ID.
     *
     *      Returns 400 Bad Request if the request data is invalid.
     *
     *      Returns 201 Created with the created notification and its
     *      resource location when successful.
     *
     */
    [HttpPost]
    public async Task<ActionResult<NotificationDto>> CreateNotification(
        [FromBody] CreateNotificationDto request)
    {
        // Validate the supplied notification data.
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Create a new notification using the request details.
        var notification = new Notification
        {
            Type = request.Type,
            Recipient = request.Recipient,
            Date = request.Date,
            Time = request.Time,
            Message = request.Message,
            Status = request.Status
        };

        // Add the new notification to the database context.
        _context.Notifications.Add(notification);

        // Save the notification and generate its database ID.
        await _context.SaveChangesAsync();

        // Map the saved notification into a DTO for the response.
        var result = new NotificationDto
        {
            NotificationId = notification.NotificationId,
            Type = notification.Type,
            Recipient = notification.Recipient,
            Date = notification.Date,
            Time = notification.Time,
            Message = notification.Message,
            Status = notification.Status
        };

        // Return 201 Created with the notification details and its resource location.
        return CreatedAtAction(
            nameof(GetNotification),
            new { id = notification.NotificationId },
            result);
    }

    #endregion

    #region Update Notification

    /*
     *          Functionality for updating an existing notification.
     *
     *      [HttpPut("{id:int}")] Allows an HTTP PUT request to update
     *      a notification using its integer ID.
     *
     *      UpdateNotificationDto Receives the updated notification details
     *      from the request body.
     *
     *      ModelState.IsValid Checks whether the supplied request data
     *      passes model validation.
     *
     *      FirstOrDefaultAsync Searches the database for the notification
     *      matching the supplied ID.
     *
     *      If the notification does not exist, the endpoint returns
     *      404 Not Found with an explanatory message.
     *
     *      Property Assignment Updates the notification fields using
     *      the values supplied in the request.
     *
     *      SaveChangesAsync Saves the updated notification record
     *      to the database.
     *
     *      Returns 400 Bad Request if the request data is invalid.
     *
     *      Returns 404 Not Found if the notification does not exist.
     *
     *      Returns 204 No Content when the update is successful.
     *
     */
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateNotification(
        int id,
        [FromBody] UpdateNotificationDto request)
    {
        // Validate the supplied notification data.
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Find the notification that needs to be updated.
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == id);

        // Check whether the requested notification exists.
        if (notification == null)
        {
            return NotFound(new
            {
                message = "Notification not found."
            });
        }

        // Update the notification fields using the request details.
        notification.Type = request.Type;
        notification.Recipient = request.Recipient;
        notification.Date = request.Date;
        notification.Time = request.Time;
        notification.Message = request.Message;
        notification.Status = request.Status;

        // Save the updated notification to the database.
        await _context.SaveChangesAsync();

        // Return 204 No Content to indicate that the update was successful.
        return NoContent();
    }

    #endregion

    #region Delete Notification

    /*
     *          Functionality for deleting an existing notification.
     *
     *      [HttpDelete("{id:int}")] Allows an HTTP DELETE request to remove
     *      a notification using its integer ID.
     *
     *      FirstOrDefaultAsync Searches the database for the notification
     *      matching the supplied ID.
     *
     *      If the notification does not exist, the endpoint returns
     *      404 Not Found with an explanatory message.
     *
     *      Remove Marks the notification entity for deletion in the
     *      database context.
     *
     *      SaveChangesAsync Applies the deletion to the database.
     *
     *      Returns 404 Not Found if the notification does not exist.
     *
     *      Returns 204 No Content when the notification is successfully deleted.
     *
     */
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        // Find the notification that needs to be deleted.
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == id);

        // Check whether the requested notification exists.
        if (notification == null)
        {
            return NotFound(new
            {
                message = "Notification not found."
            });
        }

        // Mark the notification record for deletion.
        _context.Notifications.Remove(notification);

        // Save the deletion to the database.
        await _context.SaveChangesAsync();

        // Return 204 No Content to indicate that the deletion was successful.
        return NoContent();
    }

    #endregion
}