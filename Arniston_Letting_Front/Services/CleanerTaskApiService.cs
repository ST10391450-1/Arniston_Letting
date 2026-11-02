using System.Net.Http.Json;
using Arniston_Letting_Front.Models.Tasks;
using Arniston_Letting_Front.Services.Interfaces;

namespace Arniston_Letting_Front.Services;

public class CleanerTaskApiService
    : ApiServiceBase<CleanerTaskDto, CreateCleanerTaskDto, UpdateCleanerTaskDto>, ICleanerTaskApiService
{
    public CleanerTaskApiService(HttpClient http) : base(http, "api/Tasks")
    {
    }

    #region Get Tasks By Cleaner

    /*
     *          Functionality for retrieving cleaner tasks by cleaner ID.
     *
     *      GetByCleanerAsync() Receives the ID of the cleaner whose
     *      assigned tasks need to be retrieved.
     *
     *      Http.GetFromJsonAsync() Sends an HTTP GET request to
     *      api/Tasks/cleaner/{cleanerId} and automatically deserializes
     *      the JSON response into a collection of CleanerTaskDto objects.
     *
     *      The cleaner ID is included in the API route to retrieve
     *      only tasks assigned to the selected cleaner.
     *
     *      The null-coalescing operator (??) Returns an empty collection
     *      if the API response contains no data.
     *
     *      HttpRequestException Handles HTTP communication failures
     *      by returning an empty collection.
     *
     *      Returns an IEnumerable<CleanerTaskDto> containing the
     *      cleaner's assigned tasks, or an empty collection if
     *      the request fails or no data is returned.
     *
     */

    public async Task<IEnumerable<CleanerTaskDto>> GetByCleanerAsync(int cleanerId)
    {
        try
        {
            // Retrieve all tasks assigned to the selected cleaner.
            return await Http.GetFromJsonAsync<IEnumerable<CleanerTaskDto>>(
                       $"api/Tasks/cleaner/{cleanerId}")
                   ?? Enumerable.Empty<CleanerTaskDto>();
        }
        catch (Exception apiEx) when (apiEx is HttpRequestException or TaskCanceledException)
        {
            // Return an empty collection if an HTTP communication error occurs.
            return Enumerable.Empty<CleanerTaskDto>();
        }
    }

    #endregion
}
