using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Models;

public class UserDynamoRecord
{
    public string PK { get; set; } = string.Empty;
    public string SK { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? DateOfBirth { get; set; }
    public string Status { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    
    public User ToUser() => new()
    {
        Id = Guid.Parse(UserId),
        FirstName = FirstName,
        LastName = LastName,
        DateOfBirth = DateOfBirth is null ? null : DateTime.Parse(DateOfBirth),
        Status = Enum.Parse<UserStatus>(Status),
        ContactInfo = new ContactInfo
        {
            Email = Email,
            PhoneNumber = PhoneNumber ?? null
        }
    };
}