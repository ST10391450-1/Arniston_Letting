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

    public async Task<IEnumerable<CleanerTaskDto>> GetByCleanerAsync(int cleanerId)
    {
        try
        {
            return await Http.GetFromJsonAsync<IEnumerable<CleanerTaskDto>>(
                       $"api/Tasks/cleaner/{cleanerId}")
                   ?? Enumerable.Empty<CleanerTaskDto>();
        }
        catch (HttpRequestException)
        {
            return Enumerable.Empty<CleanerTaskDto>();
        }
    }
}
