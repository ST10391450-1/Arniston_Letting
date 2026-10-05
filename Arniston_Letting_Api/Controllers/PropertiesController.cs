using Arniston_Letting_API.Data;
using Arniston_Letting_API.DTOs.Properties;
using Arniston_Letting_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arniston_Letting_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PropertiesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PropertyDto>>> GetProperties()
    {
        var properties = await _context.Properties
            .Include(p => p.Owner)
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

        return Ok(properties);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PropertyDto>> GetProperty(int id)
    {
        var property = await _context.Properties
            .Include(p => p.Owner)
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

        if (property == null)
        {
            return NotFound(new
            {
                message = "Property not found."
            });
        }

        return Ok(property);
    }

    [HttpPost]
    public async Task<ActionResult<PropertyDto>> CreateProperty(
        [FromBody] CreatePropertyDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ownerExists = await _context.Owners
            .AnyAsync(o => o.OwnerId == request.OwnerId);

        if (!ownerExists)
        {
            return BadRequest(new
            {
                message = "The selected owner does not exist."
            });
        }

        var property = new Property
        {
            PropertyName = request.PropertyName,
            Address = request.Address,
            Occupied = request.Occupied,
            OccupiedUntil = request.OccupiedUntil,
            OwnerId = request.OwnerId,
            Bedrooms = request.Bedrooms,
            Sleeps = request.Sleeps,
            RatePerNight = request.RatePerNight,
            Description = request.Description,
            Parking = request.Parking,
            Pool = request.Pool
        };

        _context.Properties.Add(property);

        await _context.SaveChangesAsync();

        var result = await _context.Properties
            .Include(p => p.Owner)
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

        return CreatedAtAction(
            nameof(GetProperty),
            new { id = property.PropertyId },
            result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProperty(
        int id,
        [FromBody] UpdatePropertyDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.PropertyId == id);

        if (property == null)
        {
            return NotFound(new
            {
                message = "Property not found."
            });
        }

        var ownerExists = await _context.Owners
            .AnyAsync(o => o.OwnerId == request.OwnerId);

        if (!ownerExists)
        {
            return BadRequest(new
            {
                message = "The selected owner does not exist."
            });
        }

        property.PropertyName = request.PropertyName;
        property.Address = request.Address;
        property.Occupied = request.Occupied;
        property.OccupiedUntil = request.OccupiedUntil;
        property.OwnerId = request.OwnerId;
        property.Bedrooms = request.Bedrooms;
        property.Sleeps = request.Sleeps;
        property.RatePerNight = request.RatePerNight;
        property.Description = request.Description;
        property.Parking = request.Parking;
        property.Pool = request.Pool;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProperty(int id)
    {
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.PropertyId == id);

        if (property == null)
        {
            return NotFound(new
            {
                message = "Property not found."
            });
        }

        var hasBookings = await _context.Bookings
            .AnyAsync(b => b.PropertyId == id);

        if (hasBookings)
        {
            return BadRequest(new
            {
                message = "The property cannot be deleted because it has bookings."
            });
        }

        var hasCleanerTasks = await _context.CleanerTasks
            .AnyAsync(t => t.LocationId == id);

        if (hasCleanerTasks)
        {
            return BadRequest(new
            {
                message = "The property cannot be deleted because it has cleaner tasks."
            });
        }

        var hasBreakages = await _context.Breakages
            .AnyAsync(b => b.LocationId == id);

        if (hasBreakages)
        {
            return BadRequest(new
            {
                message = "The property cannot be deleted because it has breakages."
            });
        }

        _context.Properties.Remove(property);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}