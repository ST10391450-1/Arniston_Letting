using Arniston_Letting_Front.Models.Tasks;
using Arniston_Letting_Front.Models.Cleaners;
using Arniston_Letting_Front.Models.Properties;
using Arniston_Letting_Front.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Arniston_Letting_Front.Pages.Admin;

public class AdminTasksModel : PageModel
{
    private readonly ICleanerTaskApiService _service;
    private readonly ICleanerApiService _cleanerApi;
    private readonly IPropertyApiService _propertyApi;

    public AdminTasksModel(ICleanerTaskApiService service, ICleanerApiService cleanerApi, IPropertyApiService propertyApi)
    {
        _service = service;
        _cleanerApi = cleanerApi;
        _propertyApi = propertyApi;
    }

    public IEnumerable<CleanerTaskDto> Items { get; set; } = Enumerable.Empty<CleanerTaskDto>();

    public List<SelectListItem> CleanerOptions { get; set; } = new();

    public List<SelectListItem> PropertyOptions { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EditId { get; set; }

    [BindProperty]
    public CreateCleanerTaskDto Input { get; set; } = new() { Date = DateTime.Today };

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
                (item.CleanerTaskId.ToString() + " " + item.CleanerName.ToString() + " " + item.LocationName.ToString())
                    .Contains(Search.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        CleanerOptions = (await _cleanerApi.GetAllAsync())
            .Select(x => new SelectListItem(x.FullName, x.CleanerId.ToString()))
            .ToList();

        PropertyOptions = (await _propertyApi.GetAllAsync())
            .Select(x => new SelectListItem(x.PropertyName, x.PropertyId.ToString()))
            .ToList();
    }

    public async Task OnGetAsync()
    {
        await LoadAsync();

        if (EditId.HasValue)
        {
            var existing = await _service.GetByIdAsync(EditId.Value);

            if (existing == null)
            {
                Error = "Task not found.";
                EditId = null;
                return;
            }

            Input = new CreateCleanerTaskDto
            {
                CleanerId = existing.CleanerId,
                LocationId = existing.LocationId,
                Date = existing.Date,
                Time = existing.Time,
                Completed = existing.Completed,
                Notes = existing.Notes
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
            var request = new UpdateCleanerTaskDto
            {
                CleanerId = Input.CleanerId,
                LocationId = Input.LocationId,
                Date = Input.Date,
                Time = Input.Time,
                Completed = Input.Completed,
                Notes = Input.Notes
            };

            var updated = await _service.UpdateAsync(EditId.Value, request);

            if (!updated)
            {
                ModelState.AddModelError(string.Empty, "The task could not be updated. Please check the details and try again.");
                await LoadAsync();
                return Page();
            }

            Message = "Task updated.";
        }
        else
        {
            var created = await _service.CreateAsync(Input);

            if (created == null)
            {
                ModelState.AddModelError(string.Empty, "The task could not be created. Please check the details and try again.");
                await LoadAsync();
                return Page();
            }

            Message = "Task created.";
        }

        return RedirectToPage(new { Search });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (deleted)
        {
            Message = "Task deleted.";
        }
        else
        {
            Error = "The task could not be deleted. It may still be referenced by other records.";
        }

        return RedirectToPage(new { Search });
    }
}
