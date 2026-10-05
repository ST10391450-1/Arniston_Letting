using Arniston_Letting_API.DTOs.Auth;
using Arniston_Letting_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Arniston_Letting_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

   /* [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _authService.RegisterAsync(request);

        if (response == null)
        {
            return BadRequest(new
            {
                success = false,
                message = "Account could not be created."
            });
        }

        return Ok(response);
    }
   */

    [AllowAnonymous]
    [EnableRateLimiting("LoginPolicy")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _authService.LoginAsync(request);

        if (response == null)
        {
            return Unauthorized(new LoginResponse
            {
                Success = false,
                Message = "Invalid email or password."
            });
        }

        return Ok(response);
    }

    [Authorize]
    [HttpPost("logout/{userId:int}")]
    public async Task<IActionResult> Logout(int userId)
    {
        var authenticatedUserId =
            User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(authenticatedUserId, out var currentUserId) ||
            currentUserId != userId)
        {
            return Forbid();
        }

        var result = await _authService.LogoutAsync(userId);

        if (!result)
        {
            return BadRequest(new
            {
                success = false,
                message = "Logout failed."
            });
        }

        return Ok(new
        {
            success = true,
            message = "Logout successful."
        });
    }
}