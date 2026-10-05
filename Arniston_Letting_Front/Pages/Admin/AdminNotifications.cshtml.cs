using Arniston_Letting_Front.Models.Notifications;
using Arniston_Letting_Front.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Arniston_Letting_Front.Pages.Admin;

public class AdminNotificationsModel : PageModel
{
    private readonly INotificationApiService _service;

    public AdminNotificationsModel(INotificationApiService service)
    {
        _service = service;
    }

    public IEnumerable<NotificationDto> Items { get; set; } = Enumerable.Empty<NotificationDto>();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EditId { get; set; }

    [BindProperty]
    public CreateNotificationDto Input { get; set; } = new() { Date = DateTime.Today };

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
                (item.NotificationId.ToString() + " " + item.Type.ToString() + " " + item.Recipient.ToString() + " " + item.Status.ToString())
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
                Error = "Notification not found.";
                EditId = null;
                return;
            }

            Input = new CreateNotificationDto
            {
                Type = existing.Type,
                Recipient = existing.Recipient,
                Date = existing.Date,
                Time = existing.Time,
                Status = existing.Status,
                Message = existing.Message
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
            var request = new UpdateNotificationDto
            {
                Type = Input.Type,
                Recipient = Input.Recipient,
                Date = Input.Date,
                Time = Input.Time,
                Status = Input.Status,
                Message = Input.Message
            };

            var updated = await _service.UpdateAsync(EditId.Value, request);

            if (!updated)
            {
                ModelState.AddModelError(string.Empty, "The notification could not be updated. Please check the details and try again.");
                await LoadAsync();
                return Page();
            }

            Message = "Notification updated.";
        }
        else
        {
            var created = await _service.CreateAsync(Input);

            if (created == null)
            {
                ModelState.AddModelError(string.Empty, "The notification could not be created. Please check the details and try again.");
                await LoadAsync();
                return Page();
            }

            Message = "Notification created.";
        }

        return RedirectToPage(new { Search });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (deleted)
        {
            Message = "Notification deleted.";
        }
        else
        {
            Error = "The notification could not be deleted. It may still be referenced by other records.";
        }

        return RedirectToPage(new { Search });
    }
}
