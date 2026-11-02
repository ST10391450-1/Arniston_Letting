using Arniston_Letting_API.DTOs.Auth;
using Arniston_Letting_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Arniston_Letting_API.Controllers;

#region Login
/*
        Controller page for the login and authentication of login details
 */


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

   
    /*
     *          Functionality for user to log in 
     * 
     *      [AllowAnonymouse] Will allow the user to access the function without being logged in with a session token. 
     *      
     *      [EnableRateLimiting] Prevents the constant logging in attempts by limiting the amount of attempts possible. 
     *      
     *      Connects credentials that a user enter to the authenitication service and will return either 401 or 200 depending on success or failure
     * 
     * 
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
#endregion


#region Log Out
    /*
     * 
     *      Allows user to logout 
     *      
     *      
     *      [Authorize] requires user to have auth token in order to access the function
     *      
     *      Checks the users ID and mathes to the user ID provided in the request.
     *      (Prevents users from logging out others) 
     *      
     *      Connects the logout request to the authentication service and returns the result
     * 
     */

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

    #endregion

}