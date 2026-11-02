using System.Net;
using System.Net.Http.Json;

namespace Arniston_Letting_Front.Services;

/*
 *          API Service Base Setup
 *
 *      ApiServiceBase Provides a reusable base service for communicating
 *      with API endpoints through HttpClient.
 *
 *      TDto Represents the Data Transfer Object returned by the API.
 *
 *      TCreate Represents the request model used when creating records.
 *
 *      TUpdate Represents the request model used when updating records.
 *
 *      where TDto : class Restricts the DTO type to reference types,
 *      allowing methods to return null when no data is available.
 * 
 *      HttpClient Handles HTTP communication between the frontend
 *      application and the backend API.
 *
 *      _route Stores the API endpoint used by the service.
 *
 *      This class provides common CRUD operations that individual
 *      API services can inherit without repeating HTTP request logic.
 *
 */

public abstract class ApiServiceBase<TDto, TCreate, TUpdate>
    where TDto : class
{
    // Provides access to the configured HttpClient.
    protected readonly HttpClient Http;

    // Stores the API endpoint used by the inherited service.
    private readonly string _route;

    // Receives the HttpClient and API route through dependency injection
    // or the constructor of the derived service.
    protected ApiServiceBase(
        HttpClient http,
        string route)
    {
        Http = http;
        _route = route;
    }

    #region Get All Records

    /*
     *          Functionality for retrieving all records from the API.
     *
     *      GetAllAsync() Sends an HTTP GET request to the configured
     *      API endpoint.
     *
     *      Http.GetAsync() Sends the request and retrieves the
     *      HTTP response from the backend.
     *
     *      Unauthorized Checks whether the request returned HTTP 401,
     *      indicating that authentication is required or invalid.
     *
     *      Forbidden Checks whether the request returned HTTP 403,
     *      indicating that the user does not have permission
     *      to access the endpoint.
     *
     *      IsSuccessStatusCode Checks whether the response contains
     *      a successful HTTP status code.
     *
     *      ReadFromJsonAsync() Converts the JSON response into
     *      a collection of TDto objects.
     *
     *      Enumerable.Empty() Returns an empty collection when
     *      the request fails or no collection can be deserialized.
     *
     *      HttpRequestException Handles HTTP communication failures.
     *
     *      Returns an IEnumerable<TDto> containing the retrieved
     *      records, or an empty collection if the request fails.
     *
     */

    public virtual async Task<IEnumerable<TDto>> GetAllAsync()
    {
        try
        {
            // Send an HTTP GET request to retrieve all records.
            var response = await Http.GetAsync(_route);

            // Return an empty collection if authentication or authorization fails.
            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.Forbidden)
            {
                return Enumerable.Empty<TDto>();
            }

            // Check whether the API returned a successful response.
            if (!response.IsSuccessStatusCode)
            {
                return Enumerable.Empty<TDto>();
            }

            // Deserialize the JSON response into a collection of DTOs.
            return await response.Content
                       .ReadFromJsonAsync<IEnumerable<TDto>>()
                   ?? Enumerable.Empty<TDto>();
        }
        catch (Exception apiEx) when (apiEx is HttpRequestException or TaskCanceledException)
        {
            // Return an empty collection if an HTTP communication error occurs.
            return Enumerable.Empty<TDto>();
        }
    }

    #endregion

    #region Get Record By ID

    /*
     *          Functionality for retrieving a single record by ID.
     *
     *      GetByIdAsync() Receives the ID of the record to retrieve.
     *
     *      Http.GetAsync() Sends an HTTP GET request to the API endpoint
     *      with the supplied ID appended to the configured route.
     *
     *      NotFound Checks whether the API returned HTTP 404,
     *      indicating that the requested record does not exist.
     *
     *      IsSuccessStatusCode Checks whether the API returned
     *      a successful HTTP response.
     *
     *      ReadFromJsonAsync() Converts the JSON response into
     *      the specified TDto object.
     *
     *      HttpRequestException Handles HTTP communication failures.
     *
     *      Returns the requested TDto object if successful,
     *      otherwise returns null.
     *
     */

    public virtual async Task<TDto?> GetByIdAsync(int id)
    {
        try
        {
            // Send an HTTP GET request using the supplied record ID.
            var response = await Http.GetAsync(
                $"{_route}/{id}");

            // Return null if the requested record does not exist.
            if (response.StatusCode ==
                HttpStatusCode.NotFound)
            {
                return null;
            }

            // Return null if the API response indicates a failure.
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            // Deserialize the JSON response into the requested DTO.
            return await response.Content
                .ReadFromJsonAsync<TDto>();
        }
        catch (Exception apiEx) when (apiEx is HttpRequestException or TaskCanceledException)
        {
            // Return null if an HTTP communication error occurs.
            return null;
        }
    }

    #endregion

    #region Create Record

    /*
     *          Functionality for creating a new record through the API.
     *
     *      CreateAsync() Receives a TCreate request object containing
     *      the information required to create a new record.
     *
     *      Http.PostAsJsonAsync() Sends an HTTP POST request to the
     *      configured API endpoint and serializes the request object
     *      into JSON.
     *
     *      IsSuccessStatusCode Checks whether the API returned
     *      a successful HTTP response.
     *
     *      ReadFromJsonAsync() Converts the JSON response into
     *      the specified TDto object.
     *
     *      HttpRequestException Handles HTTP communication failures.
     *
     *      Returns the created TDto object if successful,
     *      otherwise returns null.
     *
     */

    public virtual async Task<TDto?> CreateAsync(
        TCreate request)
    {
        try
        {
            // Send an HTTP POST request containing the creation data.
            var response = await Http.PostAsJsonAsync(
                _route,
                request);

            // Return null if the API did not successfully create the record.
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            // Deserialize the API response into the created DTO.
            return await response.Content
                .ReadFromJsonAsync<TDto>();
        }
        catch (Exception apiEx) when (apiEx is HttpRequestException or TaskCanceledException)
        {
            // Return null if an HTTP communication error occurs.
            return null;
        }
    }

    #endregion

    #region Update Record

    /*
     *          Functionality for updating an existing record through the API.
     *
     *      UpdateAsync() Receives the ID of the record to update
     *      and a TUpdate request object containing the updated data.
     *
     *      Http.PutAsJsonAsync() Sends an HTTP PUT request to the
     *      configured API endpoint with the record ID appended
     *      to the route.
     *
     *      The request object is serialized into JSON and sent
     *      in the request body.
     *
     *      IsSuccessStatusCode Checks whether the API returned
     *      a successful HTTP response.
     *
     *      HttpRequestException Handles HTTP communication failures.
     *
     *      Returns true if the API response indicates success,
     *      otherwise returns false.
     *
     */

    public virtual async Task<bool> UpdateAsync(
        int id,
        TUpdate request)
    {
        try
        {
            // Send an HTTP PUT request containing the updated record data.
            var response = await Http.PutAsJsonAsync(
                $"{_route}/{id}",
                request);

            // Return whether the API successfully processed the update.
            return response.IsSuccessStatusCode;
        }
        catch (Exception apiEx) when (apiEx is HttpRequestException or TaskCanceledException)
        {
            // Return false if an HTTP communication error occurs.
            return false;
        }
    }

    #endregion

    #region Delete Record

    /*
     *          Functionality for deleting an existing record through the API.
     *
     *      DeleteAsync() Receives the ID of the record to delete.
     *
     *      Http.DeleteAsync() Sends an HTTP DELETE request to the
     *      configured API endpoint with the record ID appended
     *      to the route.
     *
     *      IsSuccessStatusCode Checks whether the API returned
     *      a successful HTTP response.
     *
     *      HttpRequestException Handles HTTP communication failures.
     *
     *      Returns true if the API response indicates success,
     *      otherwise returns false.
     *
     */

    public virtual async Task<bool> DeleteAsync(int id)
    {
        try
        {
            // Send an HTTP DELETE request using the supplied record ID.
            var response = await Http.DeleteAsync(
                $"{_route}/{id}");

            // Return whether the API successfully processed the deletion.
            return response.IsSuccessStatusCode;
        }
        catch (Exception apiEx) when (apiEx is HttpRequestException or TaskCanceledException)
        {
            // Return false if an HTTP communication error occurs.
            return false;
        }
    }

    #endregion
}