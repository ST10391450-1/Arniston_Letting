using Arniston_Letting_API.Data;
using Arniston_Letting_API.DTOs.Reports;
using Arniston_Letting_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arniston_Letting_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReportDto>>> GetReports()
    {
        var reports = await _context.Reports
            .Select(r => new ReportDto
            {
                ReportId = r.ReportId,
                ReportType = r.ReportType,
                Date = r.Date,
                GeneratedBy = r.GeneratedBy,
                Status = r.Status,
                Description = r.Description
            })
            .OrderByDescending(r => r.Date)
            .ToListAsync();

        return Ok(reports);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReportDto>> GetReport(int id)
    {
        var report = await _context.Reports
            .Where(r => r.ReportId == id)
            .Select(r => new ReportDto
            {
                ReportId = r.ReportId,
                ReportType = r.ReportType,
                Date = r.Date,
                GeneratedBy = r.GeneratedBy,
                Status = r.Status,
                Description = r.Description
            })
            .FirstOrDefaultAsync();

        if (report == null)
        {
            return NotFound(new
            {
                message = "Report not found."
            });
        }

        return Ok(report);
    }

    [HttpPost]
    public async Task<ActionResult<ReportDto>> CreateReport(
        [FromBody] CreateReportDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var report = new Report
        {
            ReportType = request.ReportType,
            Date = request.Date,
            GeneratedBy = request.GeneratedBy,
            Status = request.Status,
            Description = request.Description
        };

        _context.Reports.Add(report);

        await _context.SaveChangesAsync();

        var result = new ReportDto
        {
            ReportId = report.ReportId,
            ReportType = report.ReportType,
            Date = report.Date,
            GeneratedBy = report.GeneratedBy,
            Status = report.Status,
            Description = report.Description
        };

        return CreatedAtAction(
            nameof(GetReport),
            new { id = report.ReportId },
            result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateReport(
        int id,
        [FromBody] UpdateReportDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var report = await _context.Reports
            .FirstOrDefaultAsync(r => r.ReportId == id);

        if (report == null)
        {
            return NotFound(new
            {
                message = "Report not found."
            });
        }

        report.ReportType = request.ReportType;
        report.Date = request.Date;
        report.GeneratedBy = request.GeneratedBy;
        report.Status = request.Status;
        report.Description = request.Description;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteReport(int id)
    {
        var report = await _context.Reports
            .FirstOrDefaultAsync(r => r.ReportId == id);

        if (report == null)
        {
            return NotFound(new
            {
                message = "Report not found."
            });
        }

        _context.Reports.Remove(report);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id:int}/generate")]
    public async Task<IActionResult> GenerateReport(int id)
    {
        var report = await _context.Reports
            .FirstOrDefaultAsync(r => r.ReportId == id);

        if (report == null)
        {
            return NotFound(new
            {
                message = "Report not found."
            });
        }

        report.Status = "Generated";
        report.Date = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            report.ReportId,
            report.ReportType,
            report.Date,
            report.GeneratedBy,
            report.Status,
            report.Description
        });
    }
}