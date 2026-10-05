namespace Arniston_Letting_Front.Models.Cleaners;

public class CleanerDto
{
    public int CleanerId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool Available { get; set; }
}