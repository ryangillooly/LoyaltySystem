using System.Text.Json.Serialization;
using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Models;

public class Permission
{
    public Guid UserId { get; set; }
    public Guid BusinessId { get; set; }
    public UserRole Role { get; set; }
}