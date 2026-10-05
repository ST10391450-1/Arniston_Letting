using System.Net.Http.Json;
using Arniston_Letting_Front.Models.Notifications;
using Arniston_Letting_Front.Services.Interfaces;

namespace Arniston_Letting_Front.Services;

public class NotificationApiService
    : ApiServiceBase<NotificationDto, CreateNotificationDto, UpdateNotificationDto>, INotificationApiService
{
    public NotificationApiService(HttpClient http) : base(http, "api/Notifications")
    {
    }
}
