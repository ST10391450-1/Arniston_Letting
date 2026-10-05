using Arniston_Letting_API.Data;
using Arniston_Letting_API.DTOs.Notifications;
using Arniston_Letting_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arniston_Letting_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public NotificationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications()
    {
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

        return Ok(notifications);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NotificationDto>> GetNotification(int id)
    {
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

        if (notification == null)
        {
            return NotFound(new
            {
                message = "Notification not found."
            });
        }

        return Ok(notification);
    }

    [HttpPost]
    public async Task<ActionResult<NotificationDto>> CreateNotification(
        [FromBody] CreateNotificationDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var notification = new Notification
        {
            Type = request.Type,
            Recipient = request.Recipient,
            Date = request.Date,
            Time = request.Time,
            Message = request.Message,
            Status = request.Status
        };

        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();

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

        return CreatedAtAction(
            nameof(GetNotification),
            new { id = notification.NotificationId },
            result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateNotification(
        int id,
        [FromBody] UpdateNotificationDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == id);

        if (notification == null)
        {
            return NotFound(new
            {
                message = "Notification not found."
            });
        }

        notification.Type = request.Type;
        notification.Recipient = request.Recipient;
        notification.Date = request.Date;
        notification.Time = request.Time;
        notification.Message = request.Message;
        notification.Status = request.Status;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == id);

        if (notification == null)
        {
            return NotFound(new
            {
                message = "Notification not found."
            });
        }

        _context.Notifications.Remove(notification);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}