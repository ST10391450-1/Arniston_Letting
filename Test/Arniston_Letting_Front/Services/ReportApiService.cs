using System.Net.Http.Json;
using Arniston_Letting_Front.Models.Reports;
using Arniston_Letting_Front.Services.Interfaces;

namespace Arniston_Letting_Front.Services;

public class ReportApiService
    : ApiServiceBase<ReportDto, CreateReportDto, UpdateReportDto>, IReportApiService
{
    public ReportApiService(HttpClient http) : base(http, "api/Reports")
    {
    }

    public async Task<ReportDto?> GenerateAsync(int id)
    {
        try
        {
            var response = await Http.GetAsync($"api/Reports/{id}/generate");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<ReportDto>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
