namespace Arniston_Letting_API.DTOs.Cleaners;

public class CreateCleanerDto
{
    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool Available { get; set; } = true;
}