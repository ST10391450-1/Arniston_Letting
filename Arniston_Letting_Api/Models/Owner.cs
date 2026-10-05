namespace Arniston_Letting_API.Models;

public class Owner
{
    public int OwnerId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? AlternativeNumber { get; set; }

    public string PreferredContactMethod { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public ICollection<Property> Properties { get; set; }
        = new List<Property>();
}