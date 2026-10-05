using Arniston_Letting_API.Data;
using Arniston_Letting_API.DTOs.Auth;
using Arniston_Letting_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Arniston_Letting_API.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;

    public AuthService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Email == request.Email && u.IsActive);

        if (user == null ||
            !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        return new LoginResponse
        {
            Success = true,
            Message = "Login successful.",
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            Token = string.Empty
        };
    }

    public async Task<LoginResponse?> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (existingUser != null)
        {
            return null;
        }

        var user = new Models.User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = "User",
            IsActive = true
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return new LoginResponse
        {
            Success = true,
            Message = "Account created successfully.",
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            Token = string.Empty
        };
    }

    public Task<bool> LogoutAsync(int userId)
    {
        return Task.FromResult(true);
    }
}