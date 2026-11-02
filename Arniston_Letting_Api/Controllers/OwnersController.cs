using Arniston_Letting_API.Data;
using Arniston_Letting_API.DTOs.Owners;
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
 *      [Route("api/[controller]")] Sets the base route for all owner
 *      endpoints to api/owners.
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
public class OwnersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OwnersController(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Get Owners

    /*
     *          Functionality for retrieving all owners.
     *
     *      [HttpGet] Allows an HTTP GET request to retrieve all owner records.
     *
     *      AsNoTracking Disables Entity Framework Core change tracking because
     *      the retrieved owner records are only being read.
     *
     *      Select Projects each owner into an OwnerDto, returning only
     *      the required owner properties.
     *
     *      OrderBy Sorts the owners alphabetically by their full name.
     *
     *      ToListAsync Executes the query asynchronously and retrieves
     *      the owner records.
     *
     *      Returns 200 OK with the list of owners.
     *
     */
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OwnerDto>>> GetOwners()
    {
        // Retrieve all owners without enabling change tracking.
        var owners = await _context.Owners
            .AsNoTracking()
            .Select(o => new OwnerDto
            {
                OwnerId = o.OwnerId,
                FullName = o.FullName,
                Email = o.Email,
                PhoneNumber = o.PhoneNumber,
                AlternativeNumber = o.AlternativeNumber,
                PreferredContactMethod = o.PreferredContactMethod,
                Notes = o.Notes
            })
            .OrderBy(o => o.FullName)
            .ToListAsync();

        // Return the retrieved owners.
        return Ok(owners);
    }

    #endregion

    #region Get Owner

    /*
     *          Functionality for retrieving an owner by ID.
     *
     *      [HttpGet("{id:int}")] Allows an HTTP GET request to retrieve
     *      a specific owner using their integer ID.
     *
     *      AsNoTracking Disables change tracking because the owner record
     *      is only being retrieved and not modified.
     *
     *      Where Filters the owners to find the record matching the
     *      supplied owner ID.
     *
     *      Select Projects the matching owner into an OwnerDto.
     *
     *      FirstOrDefaultAsync Executes the query asynchronously and returns
     *      the matching owner, or null if no record exists.
     *
     *      If the owner does not exist, the endpoint returns 404 Not Found
     *      with an explanatory message.
     *
     *      Returns 200 OK with the owner details when found.
     *
     */
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OwnerDto>> GetOwner(int id)
    {
        // Find the owner matching the supplied ID without enabling change tracking.
        var owner = await _context.Owners
            .AsNoTracking()
            .Where(o => o.OwnerId == id)
            .Select(o => new OwnerDto
            {
                OwnerId = o.OwnerId,
                FullName = o.FullName,
                Email = o.Email,
                PhoneNumber = o.PhoneNumber,
                AlternativeNumber = o.AlternativeNumber,
                PreferredContactMethod = o.PreferredContactMethod,
                Notes = o.Notes
            })
            .FirstOrDefaultAsync();

        // Check whether the requested owner exists.
        if (owner == null)
        {
            return NotFound(new
            {
                message = "Owner not found."
            });
        }

        // Return the owner details.
        return Ok(owner);
    }

    #endregion

    #region Create Owner

    /*
     *          Functionality for creating a new owner.
     *
     *      [HttpPost] Allows an HTTP POST request to create an owner record.
     *
     *      CreateOwnerDto Receives the owner details from the request body.
     *
     *      ModelState.IsValid Checks whether the supplied request data
     *      passes model validation.
     *
     *      String Processing Trims whitespace from the supplied owner
     *      information and converts the email address to lowercase.
     *
     *      Full Name Validation Checks that the owner's full name is not
     *      null, empty or whitespace.
     *
     *      Owner Creates a new owner entity using the processed request values.
     *
     *      Add Adds the new owner entity to the database context.
     *
     *      SaveChangesAsync Saves the new owner record to the database.
     *
     *      OwnerDto Creates the response object containing the saved
     *      owner details, including the generated owner ID.
     *
     *      Returns 400 Bad Request if the request data is invalid or
     *      the owner's full name is missing.
     *
     *      Returns 201 Created with the created owner and its resource
     *      location when successful.
     *
     */
    [HttpPost]
    public async Task<ActionResult<OwnerDto>> CreateOwner(
        [FromBody] CreateOwnerDto request)
    {
        // Validate the supplied owner data.
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Remove unnecessary whitespace and standardise the email address.
        var fullName = request.FullName?.Trim();
        var email = request.Email?.Trim().ToLowerInvariant();
        var phoneNumber = request.PhoneNumber?.Trim();
        var alternativeNumber = request.AlternativeNumber?.Trim();
        var preferredContactMethod = request.PreferredContactMethod?.Trim();
        var notes = request.Notes?.Trim();

        // Ensure that the owner's full name has been provided.
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return BadRequest(new
            {
                message = "Full name is required."
            });
        }

        // Create a new owner using the processed request details.
        var owner = new Owner
        {
            FullName = fullName,
            Email = email,
            PhoneNumber = phoneNumber,
            AlternativeNumber = alternativeNumber,
            PreferredContactMethod = preferredContactMethod,
            Notes = notes
        };

        // Add the new owner to the database context.
        _context.Owners.Add(owner);

        // Save the owner and generate their database ID.
        await _context.SaveChangesAsync();

        // Map the saved owner into a DTO for the response.
        var result = new OwnerDto
        {
            OwnerId = owner.OwnerId,
            FullName = owner.FullName,
            Email = owner.Email,
            PhoneNumber = owner.PhoneNumber,
            AlternativeNumber = owner.AlternativeNumber,
            PreferredContactMethod = owner.PreferredContactMethod,
            Notes = owner.Notes
        };

        // Return 201 Created with the owner details and their resource location.
        return CreatedAtAction(
            nameof(GetOwner),
            new { id = owner.OwnerId },
            result);
    }

    #endregion

    #region Update Owner

    /*
     *          Functionality for updating an existing owner.
     *
     *      [HttpPut("{id:int}")] Allows an HTTP PUT request to update
     *      an owner using their integer ID.
     *
     *      UpdateOwnerDto Receives the updated owner details from
     *      the request body.
     *
     *      ModelState.IsValid Checks whether the supplied request data
     *      passes model validation.
     *
     *      FirstOrDefaultAsync Searches the database for the owner
     *      matching the supplied ID.
     *
     *      If the owner does not exist, the endpoint returns 404 Not Found
     *      with an explanatory message.
     *
     *      String Processing Trims whitespace from the supplied owner
     *      information and converts the email address to lowercase.
     *
     *      Full Name Validation Ensures that the updated owner's full name
     *      is not null, empty or whitespace.
     *
     *      Property Assignment Updates the existing owner fields using
     *      the processed request values.
     *
     *      SaveChangesAsync Saves the updated owner record to the database.
     *
     *      Returns 400 Bad Request if the request data is invalid or
     *      the owner's full name is missing.
     *
     *      Returns 404 Not Found if the owner does not exist.
     *
     *      Returns 204 No Content when the update is successful.
     *
     */
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateOwner(
        int id,
        [FromBody] UpdateOwnerDto request)
    {
        // Validate the supplied owner data.
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Find the owner that needs to be updated.
        var owner = await _context.Owners
            .FirstOrDefaultAsync(o => o.OwnerId == id);

        // Check whether the requested owner exists.
        if (owner == null)
        {
            return NotFound(new
            {
                message = "Owner not found."
            });
        }

        // Remove unnecessary whitespace and standardise the email address.
        var fullName = request.FullName?.Trim();
        var email = request.Email?.Trim().ToLowerInvariant();
        var phoneNumber = request.PhoneNumber?.Trim();
        var alternativeNumber = request.AlternativeNumber?.Trim();
        var preferredContactMethod = request.PreferredContactMethod?.Trim();
        var notes = request.Notes?.Trim();

        // Ensure that the updated owner's full name has been provided.
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return BadRequest(new
            {
                message = "Full name is required."
            });
        }

        // Update the owner fields using the processed request details.
        owner.FullName = fullName;
        owner.Email = email;
        owner.PhoneNumber = phoneNumber;
        owner.AlternativeNumber = alternativeNumber;
        owner.PreferredContactMethod = preferredContactMethod;
        owner.Notes = notes;

        // Save the updated owner record to the database.
        await _context.SaveChangesAsync();

        // Return 204 No Content to indicate that the update was successful.
        return NoContent();
    }

    #endregion

    #region Delete Owner

    /*
     *          Functionality for deleting an existing owner.
     *
     *      [HttpDelete("{id:int}")] Allows an HTTP DELETE request to remove
     *      an owner using their integer ID.
     *
     *      FirstOrDefaultAsync Searches the database for the owner
     *      matching the supplied ID.
     *
     *      If the owner does not exist, the endpoint returns 404 Not Found
     *      with an explanatory message.
     *
     *      AsNoTracking Disables change tracking when checking whether
     *      properties are assigned to the owner.
     *
     *      AnyAsync Checks whether any property records reference the
     *      supplied owner ID.
     *
     *      Property Assignment Validation Prevents the owner from being
     *      deleted while properties are still assigned to them.
     *
     *      Remove Marks the owner entity for deletion in the database context.
     *
     *      SaveChangesAsync Applies the deletion to the database.
     *
     *      Returns 404 Not Found if the owner does not exist.
     *
     *      Returns 400 Bad Request if the owner still has properties assigned.
     *
     *      Returns 204 No Content when the owner is successfully deleted.
     *
     */
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteOwner(int id)
    {
        // Find the owner that needs to be deleted.
        var owner = await _context.Owners
            .FirstOrDefaultAsync(o => o.OwnerId == id);

        // Check whether the requested owner exists.
        if (owner == null)
        {
            return NotFound(new
            {
                message = "Owner not found."
            });
        }

        // Check whether any properties are currently assigned to this owner.
        var hasProperties = await _context.Properties
            .AsNoTracking()
            .AnyAsync(p => p.OwnerId == id);

        // Prevent deletion if the owner still has assigned properties.
        if (hasProperties)
        {
            return BadRequest(new
            {
                message = "The owner cannot be deleted because they have properties assigned to them."
            });
        }

        // Mark the owner record for deletion.
        _context.Owners.Remove(owner);

        // Save the deletion to the database.
        await _context.SaveChangesAsync();

        // Return 204 No Content to indicate that the deletion was successful.
        return NoContent();
    }

    #endregion
}