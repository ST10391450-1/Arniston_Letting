using Arniston_Letting_Front.Models.Bookings;
using Arniston_Letting_Front.Models.Properties;
using Arniston_Letting_Front.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Arniston_Letting_Front.Pages.Admin;

public class AdminBookingsModel : PageModel
{
    private readonly IBookingApiService _service;
    private readonly IPropertyApiService _propertyApi;

    public AdminBookingsModel(IBookingApiService service, IPropertyApiService propertyApi)
    {
        _service = service;
        _propertyApi = propertyApi;
    }

    public IEnumerable<BookingDto> Items { get; set; } = Enumerable.Empty<BookingDto>();

    public List<SelectListItem> PropertyOptions { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EditId { get; set; }

    [BindProperty]
    public CreateBookingDto Input { get; set; } = new() { CheckIn = DateTime.Today, CheckOut = DateTime.Today.AddDays(1) };

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
                (item.BookingId.ToString() + " " + item.PropertyName.ToString() + " " + item.BookerName.ToString())
                    .Contains(Search.Trim(), StringComparison.OrdinalIgnoreCase));
        }

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
                Error = "Booking not found.";
                EditId = null;
                return;
            }

            Input = new CreateBookingDto
            {
                PropertyId = existing.PropertyId,
                BookerName = existing.BookerName,
                CheckIn = existing.CheckIn,
                CheckOut = existing.CheckOut,
                Rate = existing.Rate
            };
        }
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (Input.CheckOut <= Input.CheckIn)
        {
            ModelState.AddModelError("Input.CheckOut", "Check-out must be after check-in.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        if (EditId.HasValue)
        {
            var request = new UpdateBookingDto
            {
                PropertyId = Input.PropertyId,
                BookerName = Input.BookerName,
                CheckIn = Input.CheckIn,
                CheckOut = Input.CheckOut,
                Rate = Input.Rate
            };

            var updated = await _service.UpdateAsync(EditId.Value, request);

            if (!updated)
            {
                ModelState.AddModelError(string.Empty, "The booking could not be updated. Please check the details and try again.");
                await LoadAsync();
                return Page();
            }

            Message = "Booking updated.";
        }
        else
        {
            var created = await _service.CreateAsync(Input);

            if (created == null)
            {
                ModelState.AddModelError(string.Empty, "The booking could not be created. Please check the details and try again.");
                await LoadAsync();
                return Page();
            }

            Message = "Booking created.";
        }

        return RedirectToPage(new { Search });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (deleted)
        {
            Message = "Booking deleted.";
        }
        else
        {
            Error = "The booking could not be deleted. It may still be referenced by other records.";
        }

        return RedirectToPage(new { Search });
    }
}
