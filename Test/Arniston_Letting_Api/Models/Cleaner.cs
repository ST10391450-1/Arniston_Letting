namespace Arniston_Letting_API.Models;

public class Cleaner
{
    public int CleanerId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool Available { get; set; } = true;

    public ICollection<CleanerTask> CleanerTasks { get; set; }
        = new List<CleanerTask>();
}