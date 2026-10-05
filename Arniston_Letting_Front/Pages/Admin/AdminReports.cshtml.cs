using Arniston_Letting_Front.Models.Reports;
using Arniston_Letting_Front.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Arniston_Letting_Front.Pages.Admin;

public class AdminReportsModel : PageModel
{
    private readonly IReportApiService _service;

    public AdminReportsModel(IReportApiService service)
    {
        _service = service;
    }

    public IEnumerable<ReportDto> Items { get; set; } = Enumerable.Empty<ReportDto>();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EditId { get; set; }

    [BindProperty]
    public CreateReportDto Input { get; set; } = new() { Date = DateTime.Today };

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public string? Error { get; set; }

    private async Task LoadAsync()
    {
        Items = await _service.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            Items = Items.Where(item =>
                (item.ReportId.ToString() + " " + item.ReportType.ToString() + " " + item.GeneratedBy.ToString() + " " + item.Status.ToString())
                    .Contains(Search.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }

    public async Task OnGetAsync()
    {
        await LoadAsync();

        if (EditId.HasValue)
        {
            var existing = await _service.GetByIdAsync(EditId.Value);

            if (existing == null)
            {
                Error = "Report not found.";
                EditId = null;
                return;
            }

            Input = new CreateReportDto
            {
                ReportType = existing.ReportType,
                Date = existing.Date,
                GeneratedBy = existing.GeneratedBy,
                Status = existing.Status,
                Description = existing.Description
            };
        }
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {

        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        if (EditId.HasValue)
        {
            var request = new UpdateReportDto
            {
                ReportType = Input.ReportType,
                Date = Input.Date,
                GeneratedBy = Input.GeneratedBy,
                Status = Input.Status,
                Description = Input.Description
            };

            var updated = await _service.UpdateAsync(EditId.Value, request);

            if (!updated)
            {
                ModelState.AddModelError(string.Empty, "The report could not be updated. Please check the details and try again.");
                await LoadAsync();
                return Page();
            }

            Message = "Report updated.";
        }
        else
        {
            var created = await _service.CreateAsync(Input);

            if (created == null)
            {
                ModelState.AddModelError(string.Empty, "The report could not be created. Please check the details and try again.");
                await LoadAsync();
                return Page();
            }

            Message = "Report created.";
        }

        return RedirectToPage(new { Search });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (deleted)
        {
            Message = "Report deleted.";
        }
        else
        {
            Error = "The report could not be deleted. It may still be referenced by other records.";
        }

        return RedirectToPage(new { Search });
    }

    public async Task<IActionResult> OnPostGenerateAsync(int id)
    {
        var report = await _service.GenerateAsync(id);

        if (report == null)
        {
            Error = "The report could not be generated.";
        }
        else
        {
            Message = "Report generated.";
        }

        return RedirectToPage(new { Search });
    }
}
