using System.Text.Json.Serialization;

namespace LoyaltySystem.Domain.Enums;

public enum RoleType
{
    SuperAdmin,
    Admin,
    Staff,
    Manager,
    Customer,
    User
}