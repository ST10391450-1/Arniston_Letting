using Arniston_Letting_API.DTOs.Notifications;

namespace Arniston_Letting_API.Services.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetAllAsync();
    Task<NotificationDto?> GetByIdAsync(int id);
    Task<NotificationDto?> CreateAsync(CreateNotificationDto request);
    Task<bool> UpdateAsync(int id, UpdateNotificationDto request);
    Task<bool> DeleteAsync(int id);
}