using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Arniston_Letting_Front.Pages.CreateAccount;

[Authorize(Roles = "Admin")]
public class CreateAccountModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CreateAccountModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public CreateAccountInput CreateAccountRequest { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToPage("/Login");
        }

        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (CreateAccountRequest.Password !=
            CreateAccountRequest.ConfirmPassword)
        {
            ModelState.AddModelError(
                "CreateAccountRequest.ConfirmPassword",
                "Passwords do not match.");

            return Page();
        }

        var token = Request.Cookies["AuthToken"];

        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToPage("/Login");
        }

        var client = _httpClientFactory.CreateClient("ArnistonApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var request = new
        {
            email = CreateAccountRequest.Email.Trim(),
            password = CreateAccountRequest.Password,
            firstName = CreateAccountRequest.FirstName.Trim(),
            lastName = CreateAccountRequest.LastName.Trim()
        };

        try
        {
            var response = await client.PostAsJsonAsync(
                "api/Auth/register",
                request);

            if (response.StatusCode ==
                System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToPage("/Login");
            }

            if (response.StatusCode ==
                System.Net.HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content
                    .ReadFromJsonAsync<ApiErrorResponse>();

                ModelState.AddModelError(
                    string.Empty,
                    error?.Message ??
                    "Unable to create the account.");

                return Page();
            }

            return RedirectToPage("/Login");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Unable to connect to the API.");

            return Page();
        }
    }

    public class CreateAccountInput
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;
    }

    private class ApiErrorResponse
    {
        public string? Message { get; set; }
    }
}