using System.Net.Http.Json;
using Arniston_Letting_Front.Models.Cleaners;
using Arniston_Letting_Front.Services.Interfaces;

namespace Arniston_Letting_Front.Services;

public class CleanerApiService
    : ApiServiceBase<CleanerDto, CreateCleanerDto, UpdateCleanerDto>, ICleanerApiService
{
    public CleanerApiService(HttpClient http) : base(http, "api/Cleaners")
    {
    }
}
