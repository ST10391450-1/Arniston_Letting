using Arniston_Letting_API.DTOs.Properties;

namespace Arniston_Letting_API.Services.Interfaces;

public interface IPropertyService
{
    Task<IEnumerable<PropertyDto>> GetAllAsync();
    Task<PropertyDto?> GetByIdAsync(int id);
    Task<PropertyDto?> CreateAsync(CreatePropertyDto request);
    Task<bool> UpdateAsync(int id, UpdatePropertyDto request);
    Task<bool> DeleteAsync(int id);
}