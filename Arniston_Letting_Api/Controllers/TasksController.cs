using Arniston_Letting_API.Data;
using Arniston_Letting_API.DTOs.Tasks;
using Arniston_Letting_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arniston_Letting_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TasksController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CleanerTaskDto>>> GetTasks()
    {
        var tasks = await _context.CleanerTasks
            .Include(t => t.Cleaner)
            .Include(t => t.Location)
            .Select(t => new CleanerTaskDto
            {
                CleanerTaskId = t.CleanerTaskId,
                CleanerId = t.CleanerId,
                CleanerName = t.Cleaner != null
                    ? t.Cleaner.FullName
                    : string.Empty,
                LocationId = t.LocationId,
                LocationName = t.Location != null
                    ? t.Location.PropertyName
                    : string.Empty,
                Date = t.Date,
                Time = t.Time,
                Completed = t.Completed,
                Notes = t.Notes
            })
            .OrderBy(t => t.Date)
            .ThenBy(t => t.Time)
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CleanerTaskDto>> GetTask(int id)
    {
        var task = await _context.CleanerTasks
            .Include(t => t.Cleaner)
            .Include(t => t.Location)
            .Where(t => t.CleanerTaskId == id)
            .Select(t => new CleanerTaskDto
            {
                CleanerTaskId = t.CleanerTaskId,
                CleanerId = t.CleanerId,
                CleanerName = t.Cleaner != null
                    ? t.Cleaner.FullName
                    : string.Empty,
                LocationId = t.LocationId,
                LocationName = t.Location != null
                    ? t.Location.PropertyName
                    : string.Empty,
                Date = t.Date,
                Time = t.Time,
                Completed = t.Completed,
                Notes = t.Notes
            })
            .FirstOrDefaultAsync();

        if (task == null)
        {
            return NotFound(new
            {
                message = "Cleaner task not found."
            });
        }

        return Ok(task);
    }

    [HttpGet("cleaner/{cleanerId:int}")]
    public async Task<ActionResult<IEnumerable<CleanerTaskDto>>> GetTasksByCleaner(
        int cleanerId)
    {
        var cleanerExists = await _context.Cleaners
            .AnyAsync(c => c.CleanerId == cleanerId);

        if (!cleanerExists)
        {
            return NotFound(new
            {
                message = "Cleaner not found."
            });
        }

        var tasks = await _context.CleanerTasks
            .Include(t => t.Cleaner)
            .Include(t => t.Location)
            .Where(t => t.CleanerId == cleanerId)
            .Select(t => new CleanerTaskDto
            {
                CleanerTaskId = t.CleanerTaskId,
                CleanerId = t.CleanerId,
                CleanerName = t.Cleaner != null
                    ? t.Cleaner.FullName
                    : string.Empty,
                LocationId = t.LocationId,
                LocationName = t.Location != null
                    ? t.Location.PropertyName
                    : string.Empty,
                Date = t.Date,
                Time = t.Time,
                Completed = t.Completed,
                Notes = t.Notes
            })
            .OrderBy(t => t.Date)
            .ThenBy(t => t.Time)
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpPost]
    public async Task<ActionResult<CleanerTaskDto>> CreateTask(
        [FromBody] CreateCleanerTaskDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var cleanerExists = await _context.Cleaners
            .AnyAsync(c => c.CleanerId == request.CleanerId);

        if (!cleanerExists)
        {
            return BadRequest(new
            {
                message = "The selected cleaner does not exist."
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

        var cleanerTask = new CleanerTask
        {
            CleanerId = request.CleanerId,
            LocationId = request.LocationId,
            Date = request.Date,
            Time = request.Time,
            Completed = request.Completed,
            Notes = request.Notes
        };

        _context.CleanerTasks.Add(cleanerTask);

        await _context.SaveChangesAsync();

        var result = await _context.CleanerTasks
            .Include(t => t.Cleaner)
            .Include(t => t.Location)
            .Where(t => t.CleanerTaskId == cleanerTask.CleanerTaskId)
            .Select(t => new CleanerTaskDto
            {
                CleanerTaskId = t.CleanerTaskId,
                CleanerId = t.CleanerId,
                CleanerName = t.Cleaner != null
                    ? t.Cleaner.FullName
                    : string.Empty,
                LocationId = t.LocationId,
                LocationName = t.Location != null
                    ? t.Location.PropertyName
                    : string.Empty,
                Date = t.Date,
                Time = t.Time,
                Completed = t.Completed,
                Notes = t.Notes
            })
            .FirstAsync();

        return CreatedAtAction(
            nameof(GetTask),
            new { id = cleanerTask.CleanerTaskId },
            result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateTask(
        int id,
        [FromBody] UpdateCleanerTaskDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var cleanerTask = await _context.CleanerTasks
            .FirstOrDefaultAsync(t => t.CleanerTaskId == id);

        if (cleanerTask == null)
        {
            return NotFound(new
            {
                message = "Cleaner task not found."
            });
        }

        var cleanerExists = await _context.Cleaners
            .AnyAsync(c => c.CleanerId == request.CleanerId);

        if (!cleanerExists)
        {
            return BadRequest(new
            {
                message = "The selected cleaner does not exist."
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

        cleanerTask.CleanerId = request.CleanerId;
        cleanerTask.LocationId = request.LocationId;
        cleanerTask.Date = request.Date;
        cleanerTask.Time = request.Time;
        cleanerTask.Completed = request.Completed;
        cleanerTask.Notes = request.Notes;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var cleanerTask = await _context.CleanerTasks
            .FirstOrDefaultAsync(t => t.CleanerTaskId == id);

        if (cleanerTask == null)
        {
            return NotFound(new
            {
                message = "Cleaner task not found."
            });
        }

        _context.CleanerTasks.Remove(cleanerTask);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}