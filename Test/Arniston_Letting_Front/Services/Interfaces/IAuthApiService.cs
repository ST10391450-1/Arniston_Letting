using Arniston_Letting_Front.Models.Auth;

namespace Arniston_Letting_Front.Services.Interfaces;

public interface IAuthApiService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);

    Task<bool> LogoutAsync(int userId);
}