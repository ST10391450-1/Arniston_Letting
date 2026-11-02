using Arniston_Letting_API.Data;
using Arniston_Letting_API.DTOs.Cleaners;
using Arniston_Letting_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arniston_Letting_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class CleanersController : ControllerBase
{
    #region Controller Setup

    /*
     *      Functionality for managing cleaners
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
     *      used to manage cleaner records.
     *
     */

    private readonly ApplicationDbContext _context;

    public CleanersController(ApplicationDbContext context)
    {
        _context = context;
    }

    #endregion

    #region Get Cleaners

    /*
     *          Functionality for retrieving all cleaners
     *
     *      [HttpGet] Allows an HTTP GET request to retrieve cleaners.
     *
     *      Retrieves all cleaner records from the database.
     *
     *      [AsNoTracking] Improves read performance because the retrieved
     *      records do not need to be tracked for database updates.
     *
     *      [Select] Converts cleaner records into CleanerDto objects
     *      containing the cleaner ID, full name, phone number, email
     *      address and availability status.
     *
     *      [OrderBy] Sorts cleaners alphabetically by their full name.
     *
     *      Returns 200 OK with the list of cleaners.
     *
     */

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CleanerDto>>> GetCleaners()
    {
        var cleaners = await _context.Cleaners
            .AsNoTracking()
            .Select(c => new CleanerDto
            {
                CleanerId = c.CleanerId,
                FullName = c.FullName,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email,
                Available = c.Available
            })
            .OrderBy(c => c.FullName)
            .ToListAsync();

        return Ok(cleaners);
    }

    /*
     *          Functionality for retrieving a single cleaner
     *
     *      [HttpGet("{id:int}")] Retrieves a cleaner using its ID.
     *
     *      Searches the database for the requested cleaner and converts
     *      the result into a CleanerDto containing the relevant details.
     *
     *      [AsNoTracking] Improves read performance because the retrieved
     *      record does not need to be tracked for database updates.
     *
     *      [Where] Filters the cleaner records using the specified ID.
     *
     *      [FirstOrDefaultAsync] Retrieves the matching cleaner or
     *      returns null if the record does not exist.
     *
     *      If the cleaner does not exist, returns 404 Not Found.
     *
     *      If the cleaner exists, returns 200 OK with the cleaner details.
     *
     */

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CleanerDto>> GetCleaner(int id)
    {
        var cleaner = await _context.Cleaners
            .AsNoTracking()
            .Where(c => c.CleanerId == id)
            .Select(c => new CleanerDto
            {
                CleanerId = c.CleanerId,
                FullName = c.FullName,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email,
                Available = c.Available
            })
            .FirstOrDefaultAsync();

        if (cleaner == null)
        {
            return NotFound(new
            {
                message = "Cleaner not found."
            });
        }

        return Ok(cleaner);
    }

    #endregion

    #region Create Cleaner

    /*
     *          Functionality for creating a new cleaner
     *
     *      [HttpPost] Allows an HTTP POST request to create a cleaner.
     *
     *      Validates the incoming request and extracts the cleaner's
     *      full name, phone number and email address.
     *
     *      [Trim] Removes unnecessary whitespace from the submitted
     *      text fields.
     *
     *      [ToLowerInvariant] Converts the email address to lowercase
     *      to maintain consistent email formatting.
     *
     *      Checks that the cleaner's full name has been provided.
     *
     *      Creates a new Cleaner record using the validated request
     *      details and assigns the requested availability status.
     *
     *      Saves the new cleaner record to the database.
     *
     *      Converts the created cleaner into a CleanerDto for the response.
     *
     *      Returns 400 Bad Request for invalid request data or a missing
     *      full name.
     *
     *      Returns 201 Created with the newly created cleaner and its
     *      location when the operation succeeds.
     *
     */

    [HttpPost]
    public async Task<ActionResult<CleanerDto>> CreateCleaner(
        [FromBody] CreateCleanerDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Clean and standardise the submitted cleaner details.
        var fullName = request.FullName?.Trim();
        var phoneNumber = request.PhoneNumber?.Trim();
        var email = request.Email?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return BadRequest(new
            {
                message = "Full name is required."
            });
        }

        // Create the cleaner record using the validated request details.
        var cleaner = new Cleaner
        {
            FullName = fullName,
            PhoneNumber = phoneNumber,
            Email = email,
            Available = request.Available
        };

        _context.Cleaners.Add(cleaner);

        await _context.SaveChangesAsync();

        // Convert the created cleaner into a DTO object for the response.
        var result = new CleanerDto
        {
            CleanerId = cleaner.CleanerId,
            FullName = cleaner.FullName,
            PhoneNumber = cleaner.PhoneNumber,
            Email = cleaner.Email,
            Available = cleaner.Available
        };

        return CreatedAtAction(
            nameof(GetCleaner),
            new { id = cleaner.CleanerId },
            result);
    }

    #endregion

    #region Update Cleaner

    /*
     *          Functionality for updating an existing cleaner
     *
     *      [HttpPut("{id:int}")] Allows an HTTP PUT request to update
     *      the cleaner identified by its ID.
     *
     *      Validates the incoming request and searches the database
     *      for the cleaner that needs to be updated.
     *
     *      If the cleaner does not exist, returns 404 Not Found.
     *
     *      Extracts and standardises the updated cleaner details.
     *
     *      [Trim] Removes unnecessary whitespace from the submitted
     *      text fields.
     *
     *      [ToLowerInvariant] Converts the email address to lowercase
     *      to maintain consistent email formatting.
     *
     *      Checks that the cleaner's full name has been provided.
     *
     *      Updates the existing cleaner with the validated details,
     *      including the full name, phone number, email address
     *      and availability status.
     *
     *      Saves the updated cleaner record to the database.
     *
     *      Returns 400 Bad Request for invalid request data or a
     *      missing full name.
     *
     *      Returns 404 Not Found if the cleaner does not exist.
     *
     *      Returns 204 No Content when the update succeeds.
     *
     */

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCleaner(
        int id,
        [FromBody] UpdateCleanerDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Find the cleaner that needs to be updated.
        var cleaner = await _context.Cleaners
            .FirstOrDefaultAsync(c => c.CleanerId == id);

        if (cleaner == null)
        {
            return NotFound(new
            {
                message = "Cleaner not found."
            });
        }

        // Clean and standardise the submitted cleaner details.
        var fullName = request.FullName?.Trim();
        var phoneNumber = request.PhoneNumber?.Trim();
        var email = request.Email?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return BadRequest(new
            {
                message = "Full name is required."
            });
        }

        // Update the existing cleaner with the validated request details.
        cleaner.FullName = fullName;
        cleaner.PhoneNumber = phoneNumber;
        cleaner.Email = email;
        cleaner.Available = request.Available;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    #endregion

    #region Delete Cleaner

    /*
     *          Functionality for deleting an existing cleaner
     *
     *      [HttpDelete("{id:int}")] Allows an HTTP DELETE request to
     *      delete the cleaner identified by its ID.
     *
     *      Searches the database for the specified cleaner.
     *
     *      If the cleaner does not exist, returns 404 Not Found.
     *
     *      If the cleaner exists, removes the record from the database
     *      and saves the changes.
     *
     *      Returns 204 No Content when the deletion succeeds.
     *
     */

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCleaner(int id)
    {
        // Find the cleaner that needs to be deleted.
        var cleaner = await _context.Cleaners
            .FirstOrDefaultAsync(c => c.CleanerId == id);

        if (cleaner == null)
        {
            return NotFound(new
            {
                message = "Cleaner not found."
            });
        }

        // Remove the cleaner record from the database.
        _context.Cleaners.Remove(cleaner);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    #endregion
}