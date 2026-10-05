using Arniston_Letting_Front.Models.Reports;

namespace Arniston_Letting_Front.Services.Interfaces;

public interface IReportApiService
{
    Task<IEnumerable<ReportDto>> GetAllAsync();

    Task<ReportDto?> GetByIdAsync(int id);

    Task<ReportDto?> CreateAsync(CreateReportDto request);

    Task<bool> UpdateAsync(int id, UpdateReportDto request);

    Task<bool> DeleteAsync(int id);

    Task<ReportDto?> GenerateAsync(int id);
}