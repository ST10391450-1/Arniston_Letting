using Arniston_Letting_API.DTOs.Tasks;

namespace Arniston_Letting_API.Services.Interfaces
{
    public interface ICleanerTaskService
    {
        Task<IEnumerable<CleanerTaskDto>> GetAllAsync();
        Task<CleanerTaskDto?> GetByIdAsync(int id);
        Task<IEnumerable<CleanerTaskDto>> GetByCleanerAsync(int cleanerId);
        Task<CleanerTaskDto?> CreateAsync(CreateCleanerTaskDto request);
        Task<bool> UpdateAsync(int id, UpdateCleanerTaskDto request);
        Task<bool> DeleteAsync(int id);
    }
}
