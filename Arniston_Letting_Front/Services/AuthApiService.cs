using System.Net.Http.Json;
using Arniston_Letting_Front.Models.Auth;
using Arniston_Letting_Front.Services.Interfaces;

namespace Arniston_Letting_Front.Services;

/*
 *          Authentication API Service Setup
 *
 *      AuthApiService Implements IAuthApiService and provides
 *      the frontend authentication functionality.
 *
 *      HttpClient Handles HTTP communication between the frontend
 *      application and the backend authentication API.
 *
 *      LoginRequest Represents the login information sent to the API.
 *
 *      LoginResponse Represents the authentication response returned
 *      by the backend, including user information and the JWT token.
 *
 *      This service handles user login and logout requests by
 *      communicating with the backend authentication endpoints.
 *
 */

public class AuthApiService : IAuthApiService
{
    // Provides access to the configured HttpClient.
    private readonly HttpClient _httpClient;

    // Receives the HttpClient through dependency injection.
    public AuthApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    #region Login User

    /*
     *          Functionality for authenticating a user through the API.
     *
     *      LoginAsync() Receives a LoginRequest containing the
     *      user's email address and password.
     *
     *      Http.PostAsJsonAsync() Sends an HTTP POST request to
     *      api/Auth/login and serializes the login request into JSON.
     *
     *      IsSuccessStatusCode Checks whether the API returned
     *      a successful HTTP response.
     *
     *      If the response is unsuccessful, the method returns null.
     *
     *      ReadFromJsonAsync() Converts the JSON response into
     *      a LoginResponse containing the authentication result,
     *      user information and JWT token.
     *
     *      Returns the LoginResponse if the request succeeds,
     *      otherwise returns null.
     *
     */

    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request)
    {
        try
        {
            // Send the login request to the backend authentication endpoint.
            var response = await _httpClient.PostAsJsonAsync(
                "api/Auth/login",
                request);

            // Return null if the API did not successfully process the login.
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            // Deserialize the API response into a LoginResponse object.
            return await response.Content
                .ReadFromJsonAsync<LoginResponse>();
        }
        catch (Exception apiEx) when (apiEx is HttpRequestException or TaskCanceledException)
        {
            // API unreachable or timed out: the login page already shows a
            // friendly "API is unavailable" message for a null response,
            // instead of crashing with an error page.
            return null;
        }
    }

    #endregion

    #region Logout User

    /*
     *          Functionality for logging out a user through the API.
     *
     *      LogoutAsync() Receives the ID of the user requesting logout.
     *
     *      Http.PostAsync() Sends an HTTP POST request to
     *      api/Auth/logout/{userId}, using the supplied user ID
     *      as part of the API route.
     *
     *      The request does not include a request body.
     *
     *      IsSuccessStatusCode Checks whether the API returned
     *      a successful HTTP response.
     *
     *      Returns true if the API response indicates success,
     *      otherwise returns false.
     *
     */

    public async Task<bool> LogoutAsync(int userId)
    {
        try
        {
            // Send the logout request using the supplied user ID.
            var response = await _httpClient.PostAsync(
                $"api/Auth/logout/{userId}",
                null);

            // Return whether the API successfully processed the logout request.
            return response.IsSuccessStatusCode;
        }
        catch (Exception apiEx) when (apiEx is HttpRequestException or TaskCanceledException)
        {
            // The local sign-out still happens in the caller, so a user can
            // always log out even if the API is down.
            return false;
        }
    }

    #endregion
}