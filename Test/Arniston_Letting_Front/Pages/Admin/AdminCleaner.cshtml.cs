using Arniston_Letting_Front.Models.Cleaners;
using Arniston_Letting_Front.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Arniston_Letting_Front.Pages.Admin;

public class AdminCleanerModel : PageModel
{
    private readonly ICleanerApiService _service;

    public AdminCleanerModel(ICleanerApiService service)
    {
        _service = service;
    }

    public IEnumerable<CleanerDto> Items { get; set; } = Enumerable.Empty<CleanerDto>();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EditId { get; set; }

    [BindProperty]
    public CreateCleanerDto Input { get; set; } = new();

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
                (item.CleanerId.ToString() + " " + item.FullName.ToString() + " " + item.Email.ToString() + " " + item.PhoneNumber.ToString())
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
                Error = "Cleaner not found.";
                EditId = null;
                return;
            }

            Input = new CreateCleanerDto
            {
                FullName = existing.FullName,
                PhoneNumber = existing.PhoneNumber,
                Email = existing.Email,
                Available = existing.Available
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
            var request = new UpdateCleanerDto
            {
                FullName = Input.FullName,
                PhoneNumber = Input.PhoneNumber,
                Email = Input.Email,
                Available = Input.Available
            };

            var updated = await _service.UpdateAsync(EditId.Value, request);

            if (!updated)
            {
                ModelState.AddModelError(string.Empty, "The cleaner could not be updated. Please check the details and try again.");
                await LoadAsync();
                return Page();
            }

            Message = "Cleaner updated.";
        }
        else
        {
            var created = await _service.CreateAsync(Input);

            if (created == null)
            {
                ModelState.AddModelError(string.Empty, "The cleaner could not be created. Please check the details and try again.");
                await LoadAsync();
                return Page();
            }

            Message = "Cleaner created.";
        }

        return RedirectToPage(new { Search });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (deleted)
        {
            Message = "Cleaner deleted.";
        }
        else
        {
            Error = "The cleaner could not be deleted. It may still be referenced by other records.";
        }

        return RedirectToPage(new { Search });
    }
}
