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
    private readonly ApplicationDbContext _context;

    public CleanersController(ApplicationDbContext context)
    {
        _context = context;
    }

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

    [HttpPost]
    public async Task<ActionResult<CleanerDto>> CreateCleaner(
        [FromBody] CreateCleanerDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

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

        var cleaner = new Cleaner
        {
            FullName = fullName,
            PhoneNumber = phoneNumber,
            Email = email,
            Available = request.Available
        };

        _context.Cleaners.Add(cleaner);

        await _context.SaveChangesAsync();

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

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCleaner(
        int id,
        [FromBody] UpdateCleanerDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var cleaner = await _context.Cleaners
            .FirstOrDefaultAsync(c => c.CleanerId == id);

        if (cleaner == null)
        {
            return NotFound(new
            {
                message = "Cleaner not found."
            });
        }

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

        cleaner.FullName = fullName;
        cleaner.PhoneNumber = phoneNumber;
        cleaner.Email = email;
        cleaner.Available = request.Available;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCleaner(int id)
    {
        var cleaner = await _context.Cleaners
            .FirstOrDefaultAsync(c => c.CleanerId == id);

        if (cleaner == null)
        {
            return NotFound(new
            {
                message = "Cleaner not found."
            });
        }

        _context.Cleaners.Remove(cleaner);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
