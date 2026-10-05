using Arniston_Letting_API.DTOs.Auth;

namespace Arniston_Letting_API.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);

    Task<bool> LogoutAsync(int userId);

    Task<LoginResponse?> RegisterAsync(RegisterRequest request);
}