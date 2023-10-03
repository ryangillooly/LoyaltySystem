using System.Text.Json.Serialization;
using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Models;

public class BusinessUserPermissions
{
    public BusinessUserPermissions(Guid businessId, List<UserPermission> permissions) =>
    (BusinessId, Permissions) = (businessId, permissions);
    
    public Guid BusinessId { get; set; }
    public List<UserPermission> Permissions { get; set; }
}
