namespace Arniston_Letting_API.Models;

public class Notification
{
    public int NotificationId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Recipient { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public TimeSpan Time { get; set; }

    public string Message { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}