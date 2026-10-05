namespace Arniston_Letting_Front.Models.Notifications;

public class UpdateNotificationDto
{
    public string Type { get; set; } = string.Empty;

    public string Recipient { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public TimeSpan Time { get; set; }

    public string Message { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}