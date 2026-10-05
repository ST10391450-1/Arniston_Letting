using Arniston_Letting_Front.Models.Owners;

namespace Arniston_Letting_Front.Services.Interfaces;

public interface IOwnerApiService
{
    Task<IEnumerable<OwnerDto>> GetAllAsync();

    Task<OwnerDto?> GetByIdAsync(int id);

    Task<OwnerDto?> CreateAsync(CreateOwnerDto request);

    Task<bool> UpdateAsync(int id, UpdateOwnerDto request);

    Task<bool> DeleteAsync(int id);
}