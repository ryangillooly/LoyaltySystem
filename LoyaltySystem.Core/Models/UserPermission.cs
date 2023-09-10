using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Models;

public class UserPermission
{
    public string   BusinessId { get; set; }
    public UserRole Permission { get; set; }
}