using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Arniston_Letting_Front.Pages
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            return User.Identity?.IsAuthenticated == true
                ? RedirectToPage("/Admin/AdminDashboard")
                : RedirectToPage("/Login/Login");
        }
    }
}
