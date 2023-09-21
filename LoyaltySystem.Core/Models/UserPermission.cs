using System.Text.Json.Serialization;
using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Models;

public class UserPermission
{
    public string? UserId { get; set; }
    public string BusinessId { get; set; }
    public UserRole Role { get; set; }
}