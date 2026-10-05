using Arniston_Letting_Front.Models.Properties;

namespace Arniston_Letting_Front.Services.Interfaces;

public interface IPropertyApiService
{
    Task<IEnumerable<PropertyDto>> GetAllAsync();

    Task<PropertyDto?> GetByIdAsync(int id);

    Task<PropertyDto?> CreateAsync(CreatePropertyDto request);

    Task<bool> UpdateAsync(int id, UpdatePropertyDto request);

    Task<bool> DeleteAsync(int id);
}