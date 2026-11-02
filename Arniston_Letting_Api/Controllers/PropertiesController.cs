using Arniston_Letting_API.Data;
using Arniston_Letting_API.DTOs.Properties;
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
 *      [Route("api/[controller]")] Sets the base route for all property
 *      endpoints to api/properties.
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
public class PropertiesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PropertiesController(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Get Properties

    /*
     *          Functionality for retrieving all properties.
     *
     *      [HttpGet] Allows an HTTP GET request to retrieve all property records.
     *
     *      AsNoTracking Disables Entity Framework Core change tracking because
     *      the retrieved property records are only being read.
     *
     *      Select Projects each property into a PropertyDto, returning
     *      the required property details.
     *
     *      OwnerName Retrieves the associated owner's full name when an owner
     *      exists. If no owner is available, an empty string is returned.
     *
     *      OrderBy Sorts the properties alphabetically by property name.
     *
     *      ToListAsync Executes the query asynchronously and retrieves
     *      the property records.
     *
     *      Returns 200 OK with the list of properties.
     *
     */
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PropertyDto>>> GetProperties()
    {
        // Retrieve all properties without enabling change tracking.
        var properties = await _context.Properties
            .AsNoTracking()
            .Select(p => new PropertyDto
            {
                PropertyId = p.PropertyId,
                PropertyName = p.PropertyName,
                Address = p.Address,
                Occupied = p.Occupied,
                OccupiedUntil = p.OccupiedUntil,
                OwnerId = p.OwnerId,
                OwnerName = p.Owner != null
                    ? p.Owner.FullName
                    : string.Empty,
                Bedrooms = p.Bedrooms,
                Sleeps = p.Sleeps,
                RatePerNight = p.RatePerNight,
                Description = p.Description,
                Parking = p.Parking,
                Pool = p.Pool
            })
            .OrderBy(p => p.PropertyName)
            .ToListAsync();

        // Return the retrieved properties.
        return Ok(properties);
    }

    #endregion

    #region Get Property

    /*
     *          Functionality for retrieving a property by ID.
     *
     *      [HttpGet("{id:int}")] Allows an HTTP GET request to retrieve
     *      a specific property using its integer ID.
     *
     *      AsNoTracking Disables change tracking because the property
     *      record is only being retrieved and not modified.
     *
     *      Where Filters the properties to find the record matching
     *      the supplied property ID.
     *
     *      Select Projects the matching property into a PropertyDto.
     *
     *      OwnerName Retrieves the associated owner's full name when an owner
     *      exists. If no owner is available, an empty string is returned.
     *
     *      FirstOrDefaultAsync Executes the query asynchronously and returns
     *      the matching property, or null if no record exists.
     *
     *      If the property does not exist, the endpoint returns 404 Not Found
     *      with an explanatory message.
     *
     *      Returns 200 OK with the property details when found.
     *
     */
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PropertyDto>> GetProperty(int id)
    {
        // Find the property matching the supplied ID without enabling change tracking.
        var property = await _context.Properties
            .AsNoTracking()
            .Where(p => p.PropertyId == id)
            .Select(p => new PropertyDto
            {
                PropertyId = p.PropertyId,
                PropertyName = p.PropertyName,
                Address = p.Address,
                Occupied = p.Occupied,
                OccupiedUntil = p.OccupiedUntil,
                OwnerId = p.OwnerId,
                OwnerName = p.Owner != null
                    ? p.Owner.FullName
                    : string.Empty,
                Bedrooms = p.Bedrooms,
                Sleeps = p.Sleeps,
                RatePerNight = p.RatePerNight,
                Description = p.Description,
                Parking = p.Parking,
                Pool = p.Pool
            })
            .FirstOrDefaultAsync();

        // Check whether the requested property exists.
        if (property == null)
        {
            return NotFound(new
            {
                message = "Property not found."
            });
        }

        // Return the property details.
        return Ok(property);
    }

    #endregion

    #region Create Property

    /*
     *          Functionality for creating a new property.
     *
     *      [HttpPost] Allows an HTTP POST request to create a property record.
     *
     *      CreatePropertyDto Receives the property details from the request body.
     *
     *      ModelState.IsValid Checks whether the supplied request data
     *      passes model validation.
     *
     *      String Processing Trims unnecessary whitespace from the property
     *      name, address and description.
     *
     *      Property Name Validation Ensures that the property name is not
     *      null, empty or whitespace.
     *
     *      Address Validation Ensures that the property address is not
     *      null, empty or whitespace.
     *
     *      Owner Validation Uses AsNoTracking and AnyAsync to check whether
     *      the selected owner exists in the database.
     *
     *      Property Creates a new property entity using the processed
     *      request values.
     *
     *      Add Adds the new property entity to the database context.
     *
     *      SaveChangesAsync Saves the new property record to the database.
     *
     *      PropertyDto Query Retrieves the newly created property and
     *      its associated owner name for the response.
     *
     *      FirstAsync Retrieves the newly created property DTO.
     *
     *      Returns 400 Bad Request if the request data is invalid, the
     *      property name or address is missing, or the selected owner
     *      does not exist.
     *
     *      Returns 201 Created with the created property and its resource
     *      location when successful.
     *
     */
    [HttpPost]
    public async Task<ActionResult<PropertyDto>> CreateProperty(
        [FromBody] CreatePropertyDto request)
    {
        // Validate the supplied property data.
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Remove unnecessary whitespace from the supplied property details.
        var propertyName = request.PropertyName?.Trim();
        var address = request.Address?.Trim();
        var description = request.Description?.Trim();

        // Ensure that the property name has been provided.
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return BadRequest(new
            {
                message = "Property name is required."
            });
        }

        // Ensure that the property address has been provided.
        if (string.IsNullOrWhiteSpace(address))
        {
            return BadRequest(new
            {
                message = "Property address is required."
            });
        }

        // Check whether the selected owner exists in the database.
        var ownerExists = await _context.Owners
            .AsNoTracking()
            .AnyAsync(o => o.OwnerId == request.OwnerId);

        // Prevent creation if the selected owner does not exist.
        if (!ownerExists)
        {
            return BadRequest(new
            {
                message = "The selected owner does not exist."
            });
        }

        // Create a new property using the processed request details.
        var property = new Property
        {
            PropertyName = propertyName,
            Address = address,
            Occupied = request.Occupied,
            OccupiedUntil = request.OccupiedUntil,
            OwnerId = request.OwnerId,
            Bedrooms = request.Bedrooms,
            Sleeps = request.Sleeps,
            RatePerNight = request.RatePerNight,
            Description = description,
            Parking = request.Parking,
            Pool = request.Pool
        };

        // Add the new property to the database context.
        _context.Properties.Add(property);

        // Save the property and generate its database ID.
        await _context.SaveChangesAsync();

        // Retrieve the newly created property with its associated owner name.
        var result = await _context.Properties
            .AsNoTracking()
            .Where(p => p.PropertyId == property.PropertyId)
            .Select(p => new PropertyDto
            {
                PropertyId = p.PropertyId,
                PropertyName = p.PropertyName,
                Address = p.Address,
                Occupied = p.Occupied,
                OccupiedUntil = p.OccupiedUntil,
                OwnerId = p.OwnerId,
                OwnerName = p.Owner != null
                    ? p.Owner.FullName
                    : string.Empty,
                Bedrooms = p.Bedrooms,
                Sleeps = p.Sleeps,
                RatePerNight = p.RatePerNight,
                Description = p.Description,
                Parking = p.Parking,
                Pool = p.Pool
            })
            .FirstAsync();

        // Return 201 Created with the property details and its resource location.
        return CreatedAtAction(
            nameof(GetProperty),
            new { id = property.PropertyId },
            result);
    }

    #endregion

    #region Update Property

    /*
     *          Functionality for updating an existing property.
     *
     *      [HttpPut("{id:int}")] Allows an HTTP PUT request to update
     *      a property using its integer ID.
     *
     *      UpdatePropertyDto Receives the updated property details
     *      from the request body.
     *
     *      ModelState.IsValid Checks whether the supplied request data
     *      passes model validation.
     *
     *      FirstOrDefaultAsync Searches the database for the property
     *      matching the supplied ID.
     *
     *      If the property does not exist, the endpoint returns
     *      404 Not Found with an explanatory message.
     *
     *      String Processing Trims unnecessary whitespace from the property
     *      name, address and description.
     *
     *      Property Name Validation Ensures that the updated property name
     *      is not null, empty or whitespace.
     *
     *      Address Validation Ensures that the updated property address
     *      is not null, empty or whitespace.
     *
     *      Owner Validation Uses AsNoTracking and AnyAsync to check whether
     *      the selected owner exists in the database.
     *
     *      Property Assignment Updates the existing property fields using
     *      the processed request values.
     *
     *      SaveChangesAsync Saves the updated property record to the database.
     *
     *      Returns 400 Bad Request if the request data is invalid, the
     *      property name or address is missing, or the selected owner
     *      does not exist.
     *
     *      Returns 404 Not Found if the property does not exist.
     *
     *      Returns 204 No Content when the update is successful.
     *
     */
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProperty(
        int id,
        [FromBody] UpdatePropertyDto request)
    {
        // Validate the supplied property data.
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Find the property that needs to be updated.
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.PropertyId == id);

        // Check whether the requested property exists.
        if (property == null)
        {
            return NotFound(new
            {
                message = "Property not found."
            });
        }

        // Remove unnecessary whitespace from the supplied property details.
        var propertyName = request.PropertyName?.Trim();
        var address = request.Address?.Trim();
        var description = request.Description?.Trim();

        // Ensure that the updated property name has been provided.
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return BadRequest(new
            {
                message = "Property name is required."
            });
        }

        // Ensure that the updated property address has been provided.
        if (string.IsNullOrWhiteSpace(address))
        {
            return BadRequest(new
            {
                message = "Property address is required."
            });
        }

        // Check whether the selected owner exists in the database.
        var ownerExists = await _context.Owners
            .AsNoTracking()
            .AnyAsync(o => o.OwnerId == request.OwnerId);

        // Prevent the update if the selected owner does not exist.
        if (!ownerExists)
        {
            return BadRequest(new
            {
                message = "The selected owner does not exist."
            });
        }

        // Update the property fields using the processed request details.
        property.PropertyName = propertyName;
        property.Address = address;
        property.Occupied = request.Occupied;
        property.OccupiedUntil = request.OccupiedUntil;
        property.OwnerId = request.OwnerId;
        property.Bedrooms = request.Bedrooms;
        property.Sleeps = request.Sleeps;
        property.RatePerNight = request.RatePerNight;
        property.Description = description;
        property.Parking = request.Parking;
        property.Pool = request.Pool;

        // Save the updated property record to the database.
        await _context.SaveChangesAsync();

        // Return 204 No Content to indicate that the update was successful.
        return NoContent();
    }

    #endregion

    #region Delete Property

    /*
     *          Functionality for deleting an existing property.
     *
     *      [HttpDelete("{id:int}")] Allows an HTTP DELETE request to remove
     *      a property using its integer ID.
     *
     *      FirstOrDefaultAsync Searches the database for the property
     *      matching the supplied ID.
     *
     *      If the property does not exist, the endpoint returns
     *      404 Not Found with an explanatory message.
     *
     *      Booking Validation Uses AsNoTracking and AnyAsync to check
     *      whether any bookings reference the property.
     *
     *      Cleaner Task Validation Uses AsNoTracking and AnyAsync to check
     *      whether any cleaner tasks reference the property through LocationId.
     *
     *      Breakage Validation Uses AsNoTracking and AnyAsync to check
     *      whether any breakage records reference the property through LocationId.
     *
     *      Relationship Validation Prevents deletion while the property
     *      is still referenced by bookings, cleaner tasks or breakages.
     *
     *      Remove Marks the property entity for deletion in the database context.
     *
     *      SaveChangesAsync Applies the deletion to the database.
     *
     *      Returns 404 Not Found if the property does not exist.
     *
     *      Returns 400 Bad Request if the property has bookings, cleaner tasks
     *      or breakages assigned to it.
     *
     *      Returns 204 No Content when the property is successfully deleted.
     *
     */
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProperty(int id)
    {
        // Find the property that needs to be deleted.
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.PropertyId == id);

        // Check whether the requested property exists.
        if (property == null)
        {
            return NotFound(new
            {
                message = "Property not found."
            });
        }

        // Check whether any bookings are associated with this property.
        var hasBookings = await _context.Bookings
            .AsNoTracking()
            .AnyAsync(b => b.PropertyId == id);

        // Prevent deletion if the property has existing bookings.
        if (hasBookings)
        {
            return BadRequest(new
            {
                message = "The property cannot be deleted because it has bookings."
            });
        }

        // Check whether any cleaner tasks are associated with this property.
        var hasCleanerTasks = await _context.CleanerTasks
            .AsNoTracking()
            .AnyAsync(t => t.LocationId == id);

        // Prevent deletion if the property has existing cleaner tasks.
        if (hasCleanerTasks)
        {
            return BadRequest(new
            {
                message = "The property cannot be deleted because it has cleaner tasks."
            });
        }

        // Check whether any breakage records are associated with this property.
        var hasBreakages = await _context.Breakages
            .AsNoTracking()
            .AnyAsync(b => b.LocationId == id);

        // Prevent deletion if the property has existing breakage records.
        if (hasBreakages)
        {
            return BadRequest(new
            {
                message = "The property cannot be deleted because it has breakages."
            });
        }

        // Mark the property record for deletion.
        _context.Properties.Remove(property);

        // Save the deletion to the database.
        await _context.SaveChangesAsync();

        // Return 204 No Content to indicate that the deletion was successful.
        return NoContent();
    }

    #endregion
}