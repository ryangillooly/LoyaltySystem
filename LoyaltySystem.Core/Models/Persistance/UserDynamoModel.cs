using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Models.Persistance;

public class UserDynamoModel
{
    public string PK { get; set; } = string.Empty;
    public string SK { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? PhoneNumber { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string Status { get; set; } = string.Empty;
}