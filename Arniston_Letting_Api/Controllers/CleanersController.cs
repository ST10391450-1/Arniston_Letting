using Arniston_Letting_API.Data;
using Arniston_Letting_API.DTOs.Cleaners;
using Arniston_Letting_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arniston_Letting_API.Controllers;

[ApiController]
[Route("api/[controller]")]
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

        var cleaner = new Cleaner
        {
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
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

        cleaner.FullName = request.FullName;
        cleaner.PhoneNumber = request.PhoneNumber;
        cleaner.Email = request.Email;
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