using Arniston_Letting_API.DTOs.Owners;

namespace Arniston_Letting_API.Services.Interfaces;

public interface IOwnerService
{
    Task<IEnumerable<OwnerDto>> GetAllAsync();
    Task<OwnerDto?> GetByIdAsync(int id);
    Task<OwnerDto?> CreateAsync(CreateOwnerDto request);
    Task<bool> UpdateAsync(int id, UpdateOwnerDto request);
    Task<bool> DeleteAsync(int id);
}