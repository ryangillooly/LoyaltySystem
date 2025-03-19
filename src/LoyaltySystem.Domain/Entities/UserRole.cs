using System;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Entities
{
    public class UserRole : Entity<UserRoleId>
    {
        // For EF Core
        private UserRole() : base(new UserRoleId(Guid.NewGuid())) { }
        
        public UserRole(UserId userId, RoleType role) : base(new UserRoleId(Guid.NewGuid()))
        {
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
            Role = role;
            CreatedAt = DateTime.UtcNow;
        }
        
        public UserId UserId { get; private set; }
        public RoleType Role { get; private set; }
        public DateTime CreatedAt { get; private set; }
        
        // For EF Core relationships
        public User? User { get; private set; }
    }
} 