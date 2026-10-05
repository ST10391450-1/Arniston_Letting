using Arniston_Letting_API.DTOs.Cleaners;

namespace Arniston_Letting_API.Services.Interfaces;

public interface ICleanerService
{
    Task<IEnumerable<CleanerDto>> GetAllAsync();
    Task<CleanerDto?> GetByIdAsync(int id);
    Task<CleanerDto?> CreateAsync(CreateCleanerDto request);
    Task<bool> UpdateAsync(int id, UpdateCleanerDto request);
    Task<bool> DeleteAsync(int id);
}