using Arniston_Letting_Front.Models.Breakages;

namespace Arniston_Letting_Front.Services.Interfaces;

public interface IBreakageApiService
{
    Task<IEnumerable<BreakageDto>> GetAllAsync();

    Task<BreakageDto?> GetByIdAsync(int id);

    Task<BreakageDto?> CreateAsync(CreateBreakageDto request);

    Task<bool> UpdateAsync(int id, UpdateBreakageDto request);

    Task<bool> DeleteAsync(int id);
}