using Arniston_Letting_Front.Models.Cleaners;

namespace Arniston_Letting_Front.Services.Interfaces;

public interface ICleanerApiService
{
    Task<IEnumerable<CleanerDto>> GetAllAsync();

    Task<CleanerDto?> GetByIdAsync(int id);

    Task<CleanerDto?> CreateAsync(CreateCleanerDto request);

    Task<bool> UpdateAsync(int id, UpdateCleanerDto request);

    Task<bool> DeleteAsync(int id);
}