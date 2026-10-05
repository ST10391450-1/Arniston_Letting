using Arniston_Letting_Front.Models.Notifications;

namespace Arniston_Letting_Front.Services.Interfaces;

public interface INotificationApiService
{
    Task<IEnumerable<NotificationDto>> GetAllAsync();

    Task<NotificationDto?> GetByIdAsync(int id);

    Task<NotificationDto?> CreateAsync(CreateNotificationDto request);

    Task<bool> UpdateAsync(int id, UpdateNotificationDto request);

    Task<bool> DeleteAsync(int id);
}