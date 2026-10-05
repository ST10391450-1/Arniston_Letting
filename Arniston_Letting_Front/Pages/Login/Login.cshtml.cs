using System.Security.Claims;
using Arniston_Letting_Front.Models.Auth;
using Arniston_Letting_Front.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Arniston_Letting_Front.Pages.Login
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IAuthApiService _authApiService;

        public LoginModel(IAuthApiService authApiService)
        {
            _authApiService = authApiService;
        }

        [BindProperty]
        public LoginRequest LoginRequest { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToPage("/Admin/AdminDashboard");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var response = await _authApiService.LoginAsync(LoginRequest);

            if (response == null || !response.Success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    response?.Message ?? "Invalid email or password, or the API is unavailable.");

                return Page();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, response.UserId.ToString()),
                new(ClaimTypes.Name, $"{response.FirstName} {response.LastName}".Trim()),
                new(ClaimTypes.Email, response.Email),
                new(ClaimTypes.Role, response.Role)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }

            return RedirectToPage("/Admin/AdminDashboard");
        }
    }
}
