using System.Net;
using System.Net.Http.Json;

namespace Arniston_Letting_Front.Services;

public abstract class ApiServiceBase<TDto, TCreate, TUpdate>
    where TDto : class
{
    protected readonly HttpClient Http;
    private readonly string _route;

    protected ApiServiceBase(HttpClient http, string route)
    {
        Http = http;
        _route = route;
    }

    public virtual async Task<IEnumerable<TDto>> GetAllAsync()
    {
        try
        {
            return await Http.GetFromJsonAsync<IEnumerable<TDto>>(_route)
                   ?? Enumerable.Empty<TDto>();
        }
        catch (HttpRequestException)
        {
            return Enumerable.Empty<TDto>();
        }
    }

    public virtual async Task<TDto?> GetByIdAsync(int id)
    {
        try
        {
            var response = await Http.GetAsync($"{_route}/{id}");
            if (response.StatusCode == HttpStatusCode.NotFound || !response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<TDto>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public virtual async Task<TDto?> CreateAsync(TCreate request)
    {
        try
        {
            var response = await Http.PostAsJsonAsync(_route, request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<TDto>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public virtual async Task<bool> UpdateAsync(int id, TUpdate request)
    {
        try
        {
            var response = await Http.PutAsJsonAsync($"{_route}/{id}", request);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var response = await Http.DeleteAsync($"{_route}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}
