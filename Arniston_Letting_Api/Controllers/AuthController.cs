using Arniston_Letting_API.DTOs.Auth;
using Arniston_Letting_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

using LoginRequest = Arniston_Letting_API.DTOs.Auth.LoginRequest;
using RegisterRequest = Arniston_Letting_API.DTOs.Auth.RegisterRequest;

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

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
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


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
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

    [HttpPost("logout/{userId:int}")]
    public async Task<IActionResult> Logout(int userId)
    {
        var result = await _authService.LogoutAsync(userId);

        if (!result)
        {
            return BadRequest(new
            {
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