using Arniston_Letting_API.Data;
using Arniston_Letting_API.DTOs.Owners;
using Arniston_Letting_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arniston_Letting_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OwnersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OwnersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OwnerDto>>> GetOwners()
    {
        var owners = await _context.Owners
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

        return Ok(owners);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OwnerDto>> GetOwner(int id)
    {
        var owner = await _context.Owners
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

        if (owner == null)
        {
            return NotFound(new
            {
                message = "Owner not found."
            });
        }

        return Ok(owner);
    }

    [HttpPost]
    public async Task<ActionResult<OwnerDto>> CreateOwner(
        [FromBody] CreateOwnerDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var owner = new Owner
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            AlternativeNumber = request.AlternativeNumber,
            PreferredContactMethod = request.PreferredContactMethod,
            Notes = request.Notes
        };

        _context.Owners.Add(owner);

        await _context.SaveChangesAsync();

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

        return CreatedAtAction(
            nameof(GetOwner),
            new { id = owner.OwnerId },
            result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateOwner(
        int id,
        [FromBody] UpdateOwnerDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var owner = await _context.Owners
            .FirstOrDefaultAsync(o => o.OwnerId == id);

        if (owner == null)
        {
            return NotFound(new
            {
                message = "Owner not found."
            });
        }

        owner.FullName = request.FullName;
        owner.Email = request.Email;
        owner.PhoneNumber = request.PhoneNumber;
        owner.AlternativeNumber = request.AlternativeNumber;
        owner.PreferredContactMethod = request.PreferredContactMethod;
        owner.Notes = request.Notes;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteOwner(int id)
    {
        var owner = await _context.Owners
            .FirstOrDefaultAsync(o => o.OwnerId == id);

        if (owner == null)
        {
            return NotFound(new
            {
                message = "Owner not found."
            });
        }

        var hasProperties = await _context.Properties
            .AnyAsync(p => p.OwnerId == id);

        if (hasProperties)
        {
            return BadRequest(new
            {
                message = "The owner cannot be deleted because they have properties assigned to them."
            });
        }

        _context.Owners.Remove(owner);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}