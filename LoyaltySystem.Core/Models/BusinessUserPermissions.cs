using System.Text.Json.Serialization;
using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Models;

public class BusinessUserPermissions
{
    public BusinessUserPermissions(Guid businessId, Guid userId, UserRole role) =>
    (BusinessId, UserId, Role) = (businessId, userId, role);
    

    public Guid BusinessId { get; set; }
    public Guid UserId { get; set; }
    public UserRole Role { get; set; }
}

public class UserPermissions
{
    public UserPermissions(Guid userId, UserRole role) => 
        (UserId, Role) = (userId, role);
    public Guid UserId { get; set; }
    public UserRole Role { get; set; }
}
