using Arniston_Letting_Front.Models.Properties;
using Arniston_Letting_Front.Models.Owners;
using Arniston_Letting_Front.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Arniston_Letting_Front.Pages.Admin;

public class AdminPropertiesModel : PageModel
{
    private readonly IPropertyApiService _service;
    private readonly IOwnerApiService _ownerApi;

    public AdminPropertiesModel(IPropertyApiService service, IOwnerApiService ownerApi)
    {
        _service = service;
        _ownerApi = ownerApi;
    }

    public IEnumerable<PropertyDto> Items { get; set; } = Enumerable.Empty<PropertyDto>();

    public List<SelectListItem> OwnerOptions { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EditId { get; set; }

    [BindProperty]
    public CreatePropertyDto Input { get; set; } = new();

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
                (item.PropertyId.ToString() + " " + item.PropertyName.ToString() + " " + item.OwnerName.ToString() + " " + item.Address.ToString())
                    .Contains(Search.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        OwnerOptions = (await _ownerApi.GetAllAsync())
            .Select(x => new SelectListItem(x.FullName, x.OwnerId.ToString()))
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
                Error = "Property not found.";
                EditId = null;
                return;
            }

            Input = new CreatePropertyDto
            {
                PropertyName = existing.PropertyName,
                Address = existing.Address,
                OwnerId = existing.OwnerId,
                Bedrooms = existing.Bedrooms,
                Sleeps = existing.Sleeps,
                RatePerNight = existing.RatePerNight,
                Occupied = existing.Occupied,
                OccupiedUntil = existing.OccupiedUntil,
                Parking = existing.Parking,
                Pool = existing.Pool,
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
            var request = new UpdatePropertyDto
            {
                PropertyName = Input.PropertyName,
                Address = Input.Address,
                OwnerId = Input.OwnerId,
                Bedrooms = Input.Bedrooms,
                Sleeps = Input.Sleeps,
                RatePerNight = Input.RatePerNight,
                Occupied = Input.Occupied,
                OccupiedUntil = Input.OccupiedUntil,
                Parking = Input.Parking,
                Pool = Input.Pool,
                Description = Input.Description
            };

            var updated = await _service.UpdateAsync(EditId.Value, request);

            if (!updated)
            {
                ModelState.AddModelError(string.Empty, "The property could not be updated. Please check the details and try again.");
                await LoadAsync();
                return Page();
            }

            Message = "Property updated.";
        }
        else
        {
            var created = await _service.CreateAsync(Input);

            if (created == null)
            {
                ModelState.AddModelError(string.Empty, "The property could not be created. Please check the details and try again.");
                await LoadAsync();
                return Page();
            }

            Message = "Property created.";
        }

        return RedirectToPage(new { Search });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (deleted)
        {
            Message = "Property deleted.";
        }
        else
        {
            Error = "The property could not be deleted. It may still be referenced by other records.";
        }

        return RedirectToPage(new { Search });
    }
}
