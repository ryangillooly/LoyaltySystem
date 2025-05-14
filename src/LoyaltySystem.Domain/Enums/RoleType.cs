using System.Text.Json.Serialization;

namespace LoyaltySystem.Domain.Enums;

public enum RoleType
{
    User = 1,
    Customer = 2,
    Staff = 3,
    Manager = 4,
    Admin = 5,
    SuperAdmin = 6,
}