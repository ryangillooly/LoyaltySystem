using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Models;

public class UserPermission
{
    public Guid UserId { get; set; }
    public UserRole Role { get; set; }
}