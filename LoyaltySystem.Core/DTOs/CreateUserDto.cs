using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Dtos;

public class CreateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public ContactInfo ContactInfo { get; set; } = new ();
    public DateTime? DateOfBirth { get; set; }
}