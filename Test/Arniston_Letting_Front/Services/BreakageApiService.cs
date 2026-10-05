using System.Net.Http.Json;
using Arniston_Letting_Front.Models.Breakages;
using Arniston_Letting_Front.Services.Interfaces;

namespace Arniston_Letting_Front.Services;

public class BreakageApiService
    : ApiServiceBase<BreakageDto, CreateBreakageDto, UpdateBreakageDto>, IBreakageApiService
{
    public BreakageApiService(HttpClient http) : base(http, "api/Breakages")
    {
    }
}
