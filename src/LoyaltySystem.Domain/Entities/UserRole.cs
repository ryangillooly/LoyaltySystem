using System;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Entities;

public class UserRole : Entity<UserRoleId>
{
    public UserRole
    (
        UserId userId, 
        RoleType role
    ) 
    : base(new UserRoleId())
    {
        Role = role;
        CreatedAt = DateTime.UtcNow;
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
    }
        
    public UserId UserId { get; set; }
    public RoleType Role { get; set; }
}