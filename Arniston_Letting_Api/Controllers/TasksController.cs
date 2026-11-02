using Arniston_Letting_API.Data;
using Arniston_Letting_API.DTOs.Tasks;
using Arniston_Letting_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arniston_Letting_API.Controllers;

/*
 *          Tasks Controller Setup
 *
 *      [ApiController] Identifies this class as an API controller and
 *      enables automatic API-specific behaviour.
 *
 *      [Route("api/[controller]")] Defines the base API route as
 *      api/Tasks.
 *
 *      [Authorize(Roles = "Admin")] Restricts access to all endpoints
 *      in this controller to users with the Admin role.
 *
 *      ApplicationDbContext Provides access to the database tables
 *      used to manage cleaner tasks, cleaners and properties.
 *
 */

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class TasksController : ControllerBase
{
    // Provides access to the application's database.
    private readonly ApplicationDbContext _context;

    // Receives the database context through dependency injection.
    public TasksController(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Get Tasks

    /*
     *          Functionality for retrieving all cleaner tasks.
     *
     *      [HttpGet] Allows an HTTP GET request to retrieve all
     *      cleaner tasks.
     *
     *      AsNoTracking() Retrieves records without tracking them
     *      because this endpoint only reads data.
     *
     *      Select() Converts each CleanerTask database record into
     *      a CleanerTaskDto.
     *
     *      CleanerName and LocationName retrieve the associated
     *      cleaner and property names when available. If either
     *      navigation property is null, an empty string is returned.
     *
     *      OrderBy() Sorts the tasks by date in ascending order.
     *
     *      ThenBy() Sorts tasks with the same date by time
     *      in ascending order.
     *
     *      ToListAsync() Executes the query asynchronously and
     *      retrieves all matching records.
     *
     *      Returns 200 OK with the collection of cleaner tasks.
     *
     */

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CleanerTaskDto>>> GetTasks()
    {
        // Retrieve all cleaner tasks and convert them into DTOs.
        var tasks = await _context.CleanerTasks
            .AsNoTracking()
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

        // Return the retrieved tasks.
        return Ok(tasks);
    }

    #endregion

    #region Get Task By ID

    /*
     *          Functionality for retrieving a cleaner task by ID.
     *
     *      [HttpGet("{id:int}")] Allows an HTTP GET request using
     *      the cleaner task ID as a route parameter.
     *
     *      AsNoTracking() Retrieves the task without tracking it
     *      because this endpoint only reads data.
     *
     *      Where() Filters the database records to find the task
     *      matching the supplied ID.
     *
     *      Select() Converts the matching database record into
     *      a CleanerTaskDto, including the associated cleaner
     *      and property names when available.
     *
     *      FirstOrDefaultAsync() Retrieves the first matching record
     *      or returns null if no matching task exists.
     *
     *      Returns 404 Not Found if the cleaner task does not exist.
     *
     *      Returns 200 OK with the requested cleaner task if found.
     *
     */

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CleanerTaskDto>> GetTask(int id)
    {
        // Find the cleaner task matching the supplied ID.
        var task = await _context.CleanerTasks
            .AsNoTracking()
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

        // Check whether the requested task exists.
        if (task == null)
        {
            return NotFound(new
            {
                message = "Cleaner task not found."
            });
        }

        // Return the requested cleaner task.
        return Ok(task);
    }

    #endregion

    #region Get Tasks By Cleaner

    /*
     *          Functionality for retrieving tasks assigned to a cleaner.
     *
     *      [HttpGet("cleaner/{cleanerId:int}")] Allows an HTTP GET
     *      request using the cleaner ID as a route parameter.
     *
     *      AnyAsync() Checks whether the cleaner exists in the database.
     *
     *      If the cleaner does not exist, the endpoint returns
     *      404 Not Found.
     *
     *      AsNoTracking() Retrieves task records without tracking
     *      because this endpoint only reads data.
     *
     *      Where() Filters the cleaner tasks to include only those
     *      assigned to the supplied cleaner ID.
     *
     *      Select() Converts each matching database record into
     *      a CleanerTaskDto, including the cleaner and property names
     *      when available.
     *
     *      OrderBy() Sorts the tasks by date in ascending order.
     *
     *      ThenBy() Sorts tasks with the same date by time
     *      in ascending order.
     *
     *      ToListAsync() Executes the query asynchronously and
     *      retrieves the matching tasks.
     *
     *      Returns 200 OK with the cleaner's tasks.
     *
     */

    [HttpGet("cleaner/{cleanerId:int}")]
    public async Task<ActionResult<IEnumerable<CleanerTaskDto>>> GetTasksByCleaner(
        int cleanerId)
    {
        // Check whether the selected cleaner exists in the database.
        var cleanerExists = await _context.Cleaners
            .AsNoTracking()
            .AnyAsync(c => c.CleanerId == cleanerId);

        // Return an error if the cleaner does not exist.
        if (!cleanerExists)
        {
            return NotFound(new
            {
                message = "Cleaner not found."
            });
        }

        // Retrieve all tasks assigned to the selected cleaner.
        var tasks = await _context.CleanerTasks
            .AsNoTracking()
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

        // Return the tasks assigned to the cleaner.
        return Ok(tasks);
    }

    #endregion

    #region Create Task

    /*
     *          Functionality for creating a new cleaner task.
     *
     *      [HttpPost] Allows an HTTP POST request to create
     *      a new cleaner task.
     *
     *      CreateCleanerTaskDto Receives the task information
     *      supplied in the request body.
     *
     *      ModelState.IsValid Checks whether the supplied request
     *      data passes model validation.
     *
     *      AnyAsync() Checks whether the selected cleaner exists
     *      in the database.
     *
     *      AnyAsync() Also checks whether the selected property
     *      exists in the database.
     *
     *      If either related record does not exist, the endpoint
     *      returns 400 Bad Request.
     *
     *      Trim() Removes leading and trailing whitespace from
     *      the optional task notes.
     *
     *      A new CleanerTask entity is created using the supplied
     *      cleaner, property, date, time, completion status and notes.
     *
     *      Add() Adds the new task to the database context.
     *
     *      SaveChangesAsync() Saves the new task to the database.
     *
     *      FirstAsync() Retrieves the newly created task and converts
     *      it into a CleanerTaskDto.
     *
     *      CreatedAtAction() Returns 201 Created and includes
     *      a location pointing to the GetTask endpoint for the
     *      newly created task.
     *
     */

    [HttpPost]
    public async Task<ActionResult<CleanerTaskDto>> CreateTask(
        [FromBody] CreateCleanerTaskDto request)
    {
        // Check whether the supplied request data is valid.
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check whether the selected cleaner exists in the database.
        var cleanerExists = await _context.Cleaners
            .AsNoTracking()
            .AnyAsync(c => c.CleanerId == request.CleanerId);

        // Prevent task creation if the selected cleaner does not exist.
        if (!cleanerExists)
        {
            return BadRequest(new
            {
                message = "The selected cleaner does not exist."
            });
        }

        // Check whether the selected property exists in the database.
        var locationExists = await _context.Properties
            .AsNoTracking()
            .AnyAsync(p => p.PropertyId == request.LocationId);

        // Prevent task creation if the selected property does not exist.
        if (!locationExists)
        {
            return BadRequest(new
            {
                message = "The selected property does not exist."
            });
        }

        // Remove leading and trailing whitespace from the optional notes.
        var notes = request.Notes?.Trim();

        // Create a new cleaner task using the supplied request information.
        var cleanerTask = new CleanerTask
        {
            CleanerId = request.CleanerId,
            LocationId = request.LocationId,
            Date = request.Date,
            Time = request.Time,
            Completed = request.Completed,
            Notes = notes
        };

        // Add the new cleaner task to the database context.
        _context.CleanerTasks.Add(cleanerTask);

        // Save the new cleaner task to the database.
        await _context.SaveChangesAsync();

        // Retrieve the newly created task and convert it into a DTO.
        var result = await _context.CleanerTasks
            .AsNoTracking()
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

        // Return 201 Created with the new task and its retrieval location.
        return CreatedAtAction(
            nameof(GetTask),
            new { id = cleanerTask.CleanerTaskId },
            result);
    }

    #endregion

    #region Update Task

    /*
     *          Functionality for updating an existing cleaner task.
     *
     *      [HttpPut("{id:int}")] Allows an HTTP PUT request using
     *      the cleaner task ID as a route parameter.
     *
     *      UpdateCleanerTaskDto Receives the updated task information
     *      supplied in the request body.
     *
     *      ModelState.IsValid Checks whether the supplied request
     *      data passes model validation.
     *
     *      FirstOrDefaultAsync() Finds the cleaner task matching
     *      the supplied ID.
     *
     *      If the task does not exist, the endpoint returns
     *      404 Not Found.
     *
     *      AnyAsync() Checks whether the selected cleaner exists
     *      in the database.
     *
     *      AnyAsync() Also checks whether the selected property
     *      exists in the database.
     *
     *      If either related record does not exist, the endpoint
     *      returns 400 Bad Request.
     *
     *      The existing cleaner task is updated with the supplied
     *      cleaner, property, date, time, completion status and notes.
     *
     *      Trim() Removes leading and trailing whitespace from
     *      the optional task notes.
     *
     *      SaveChangesAsync() Saves the updated task to the database.
     *
     *      Returns 204 No Content when the update is successful.
     *
     */

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateTask(
        int id,
        [FromBody] UpdateCleanerTaskDto request)
    {
        // Check whether the supplied request data is valid.
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Find the cleaner task that needs to be updated.
        var cleanerTask = await _context.CleanerTasks
            .FirstOrDefaultAsync(t => t.CleanerTaskId == id);

        // Check whether the requested task exists.
        if (cleanerTask == null)
        {
            return NotFound(new
            {
                message = "Cleaner task not found."
            });
        }

        // Check whether the selected cleaner exists in the database.
        var cleanerExists = await _context.Cleaners
            .AsNoTracking()
            .AnyAsync(c => c.CleanerId == request.CleanerId);

        // Prevent the update if the selected cleaner does not exist.
        if (!cleanerExists)
        {
            return BadRequest(new
            {
                message = "The selected cleaner does not exist."
            });
        }

        // Check whether the selected property exists in the database.
        var locationExists = await _context.Properties
            .AsNoTracking()
            .AnyAsync(p => p.PropertyId == request.LocationId);

        // Prevent the update if the selected property does not exist.
        if (!locationExists)
        {
            return BadRequest(new
            {
                message = "The selected property does not exist."
            });
        }

        // Update the existing task with the supplied information.
        cleanerTask.CleanerId = request.CleanerId;
        cleanerTask.LocationId = request.LocationId;
        cleanerTask.Date = request.Date;
        cleanerTask.Time = request.Time;
        cleanerTask.Completed = request.Completed;
        cleanerTask.Notes = request.Notes?.Trim();

        // Save the updated cleaner task to the database.
        await _context.SaveChangesAsync();

        // Return 204 No Content to confirm the update was successful.
        return NoContent();
    }

    #endregion

    #region Delete Task

    /*
     *          Functionality for deleting an existing cleaner task.
     *
     *      [HttpDelete("{id:int}")] Allows an HTTP DELETE request
     *      using the cleaner task ID as a route parameter.
     *
     *      FirstOrDefaultAsync() Finds the cleaner task matching
     *      the supplied ID.
     *
     *      If the task does not exist, the endpoint returns
     *      404 Not Found.
     *
     *      Remove() Marks the selected cleaner task for deletion
     *      in the database context.
     *
     *      SaveChangesAsync() Executes the deletion in the database.
     *
     *      Returns 204 No Content when the deletion is successful.
     *
     */

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        // Find the cleaner task that needs to be deleted.
        var cleanerTask = await _context.CleanerTasks
            .FirstOrDefaultAsync(t => t.CleanerTaskId == id);

        // Check whether the requested task exists.
        if (cleanerTask == null)
        {
            return NotFound(new
            {
                message = "Cleaner task not found."
            });
        }

        // Mark the selected cleaner task for deletion.
        _context.CleanerTasks.Remove(cleanerTask);

        // Save the deletion to the database.
        await _context.SaveChangesAsync();

        // Return 204 No Content to confirm the deletion was successful.
        return NoContent();
    }

    #endregion
}