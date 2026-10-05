using Arniston_Letting_Front.Models.Breakages;
using Arniston_Letting_Front.Models.Properties;
using Arniston_Letting_Front.Models.Bookings;
using Arniston_Letting_Front.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Arniston_Letting_Front.Pages.Admin;

public class AdminBreakagesModel : PageModel
{
    private readonly IBreakageApiService _service;
    private readonly IPropertyApiService _propertyApi;
    private readonly IBookingApiService _bookingApi;

    public AdminBreakagesModel(IBreakageApiService service, IPropertyApiService propertyApi, IBookingApiService bookingApi)
    {
        _service = service;
        _propertyApi = propertyApi;
        _bookingApi = bookingApi;
    }

    public IEnumerable<BreakageDto> Items { get; set; } = Enumerable.Empty<BreakageDto>();

    public List<SelectListItem> PropertyOptions { get; set; } = new();

    public List<SelectListItem> BookingOptions { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EditId { get; set; }

    [BindProperty]
    public CreateBreakageDto Input { get; set; } = new() { Date = DateTime.Today };

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
                (item.BreakageId.ToString() + " " + item.LocationName.ToString() + " " + item.ReportedBy.ToString())
                    .Contains(Search.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        PropertyOptions = (await _propertyApi.GetAllAsync())
            .Select(x => new SelectListItem(x.PropertyName, x.PropertyId.ToString()))
            .ToList();

        BookingOptions = (await _bookingApi.GetAllAsync())
            .Select(x => new SelectListItem($"#{x.BookingId} - {x.BookerName} ({x.PropertyName})", x.BookingId.ToString()))
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
                Error = "Breakage not found.";
                EditId = null;
                return;
            }

            Input = new CreateBreakageDto
            {
                LocationId = existing.LocationId,
                BookingId = existing.BookingId,
                Date = existing.Date,
                Time = existing.Time,
                ReportedBy = existing.ReportedBy,
                Resolved = existing.Resolved,
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
            var request = new UpdateBreakageDto
            {
                LocationId = Input.LocationId,
                BookingId = Input.BookingId,
                Date = Input.Date,
                Time = Input.Time,
                ReportedBy = Input.ReportedBy,
                Resolved = Input.Resolved,
                Notes = Input.Notes
            };

            var updated = await _service.UpdateAsync(EditId.Value, request);

            if (!updated)
            {
                ModelState.AddModelError(string.Empty, "The breakage could not be updated. Please check the details and try again.");
                await LoadAsync();
                return Page();
            }

            Message = "Breakage updated.";
        }
        else
        {
            var created = await _service.CreateAsync(Input);

            if (created == null)
            {
                ModelState.AddModelError(string.Empty, "The breakage could not be created. Please check the details and try again.");
                await LoadAsync();
                return Page();
            }

            Message = "Breakage created.";
        }

        return RedirectToPage(new { Search });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (deleted)
        {
            Message = "Breakage deleted.";
        }
        else
        {
            Error = "The breakage could not be deleted. It may still be referenced by other records.";
        }

        return RedirectToPage(new { Search });
    }
}
