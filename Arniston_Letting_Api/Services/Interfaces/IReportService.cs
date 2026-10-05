using Arniston_Letting_API.DTOs.Reports;

namespace Arniston_Letting_API.Services.Interfaces;

public interface IReportService
{
    Task<IEnumerable<ReportDto>> GetAllAsync();
    Task<ReportDto?> GetByIdAsync(int id);
    Task<ReportDto?> CreateAsync(CreateReportDto request);
    Task<bool> UpdateAsync(int id, UpdateReportDto request);
    Task<bool> DeleteAsync(int id);
    Task<ReportDto?> GenerateAsync(int id);
}