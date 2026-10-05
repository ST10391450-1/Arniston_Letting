using System.Net.Http.Json;
using Arniston_Letting_Front.Models.Properties;
using Arniston_Letting_Front.Services.Interfaces;

namespace Arniston_Letting_Front.Services;

public class PropertyApiService
    : ApiServiceBase<PropertyDto, CreatePropertyDto, UpdatePropertyDto>, IPropertyApiService
{
    public PropertyApiService(HttpClient http) : base(http, "api/Properties")
    {
    }
}
