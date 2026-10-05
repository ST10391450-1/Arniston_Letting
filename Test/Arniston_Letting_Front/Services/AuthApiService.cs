using System.Net.Http.Json;
using Arniston_Letting_Front.Models.Auth;
using Arniston_Letting_Front.Services.Interfaces;

namespace Arniston_Letting_Front.Services;

public class AuthApiService : IAuthApiService
{
    private readonly HttpClient _httpClient;

    public AuthApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/Auth/login",
            request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content
            .ReadFromJsonAsync<LoginResponse>();
    }

    public async Task<bool> LogoutAsync(int userId)
    {
        var response = await _httpClient.PostAsync(
            $"api/Auth/logout/{userId}",
            null);

        return response.IsSuccessStatusCode;
    }
}