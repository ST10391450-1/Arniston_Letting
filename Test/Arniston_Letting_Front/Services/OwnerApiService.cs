using System.Net.Http.Json;
using Arniston_Letting_Front.Models.Owners;
using Arniston_Letting_Front.Services.Interfaces;

namespace Arniston_Letting_Front.Services;

public class OwnerApiService
    : ApiServiceBase<OwnerDto, CreateOwnerDto, UpdateOwnerDto>, IOwnerApiService
{
    public OwnerApiService(HttpClient http) : base(http, "api/Owners")
    {
    }
}
