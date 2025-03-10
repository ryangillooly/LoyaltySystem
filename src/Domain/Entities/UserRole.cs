using System;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Entities
{
    /// <summary>
    /// Represents a role assigned to a user.
    /// </summary>
    public class UserRole
    {
        public UserId UserId { get; private set; }
        public RoleType Role { get; private set; }
        public DateTime CreatedAt { get; private set; }
        
        // Navigation property
        public virtual User User { get; private set; }
        
        // Private constructor for EF Core
        private UserRole() { }
        
        public UserRole(UserId userId, RoleType role)
        {
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
            Role = role;
            CreatedAt = DateTime.UtcNow;
        }
    }
} 