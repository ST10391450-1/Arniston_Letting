using Arniston_Letting_Front.Models.Owners;
using Arniston_Letting_Front.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Arniston_Letting_Front.Pages.Admin;

public class AdminOwnersModel : PageModel
{
    private readonly IOwnerApiService _service;

    public AdminOwnersModel(IOwnerApiService service)
    {
        _service = service;
    }

    public IEnumerable<OwnerDto> Items { get; set; } = Enumerable.Empty<OwnerDto>();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EditId { get; set; }

    [BindProperty]
    public CreateOwnerDto Input { get; set; } = new();

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
                (item.OwnerId.ToString() + " " + item.FullName.ToString() + " " + item.Email.ToString() + " " + item.PhoneNumber.ToString())
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
                Error = "Owner not found.";
                EditId = null;
                return;
            }

            Input = new CreateOwnerDto
            {
                FullName = existing.FullName,
                Email = existing.Email,
                PhoneNumber = existing.PhoneNumber,
                AlternativeNumber = existing.AlternativeNumber,
                PreferredContactMethod = existing.PreferredContactMethod,
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
            var request = new UpdateOwnerDto
            {
                FullName = Input.FullName,
                Email = Input.Email,
                PhoneNumber = Input.PhoneNumber,
                AlternativeNumber = Input.AlternativeNumber,
                PreferredContactMethod = Input.PreferredContactMethod,
                Notes = Input.Notes
            };

            var updated = await _service.UpdateAsync(EditId.Value, request);

            if (!updated)
            {
                ModelState.AddModelError(string.Empty, "The owner could not be updated. Please check the details and try again.");
                await LoadAsync();
                return Page();
            }

            Message = "Owner updated.";
        }
        else
        {
            var created = await _service.CreateAsync(Input);

            if (created == null)
            {
                ModelState.AddModelError(string.Empty, "The owner could not be created. Please check the details and try again.");
                await LoadAsync();
                return Page();
            }

            Message = "Owner created.";
        }

        return RedirectToPage(new { Search });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (deleted)
        {
            Message = "Owner deleted.";
        }
        else
        {
            Error = "The owner could not be deleted. It may still be referenced by other records.";
        }

        return RedirectToPage(new { Search });
    }
}
