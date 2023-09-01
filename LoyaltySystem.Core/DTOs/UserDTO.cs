using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Models;

public class UserDTO
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public ContactInfo ContactInfo { get; set; }
    public string PasswordHash { get; set; } // Note: Store hashed password, not plaintext
    // Other fields like Firstname, Lastname, etc.
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }
}