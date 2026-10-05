using Arniston_Letting_Front.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Arniston_Letting_Front.Pages;

public class LogoutModel : PageModel
{
    private readonly IAuthApiService _authApiService;

    public LogoutModel(IAuthApiService authApiService)
    {
        _authApiService = authApiService;
    }

    public IActionResult OnGet() => RedirectToPage("/Admin/AdminDashboard");

    public async Task<IActionResult> OnPostAsync()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (int.TryParse(idClaim, out var userId))
        {
            await _authApiService.LogoutAsync(userId);
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToPage("/Login/Login");
    }
}
