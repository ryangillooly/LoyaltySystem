using System;
using System.Collections.Generic;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Entities
{
    /// <summary>
    /// Represents a user in the system.
    /// </summary>
    public class User
    {
        private readonly List<UserRole> _roles;

        public UserId Id { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public string PasswordSalt { get; private set; }
        public CustomerId? CustomerId { get; private set; }
        public UserStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        
        // Navigation properties
        public virtual Customer Customer { get; private set; }
        
        // Collection navigation property
        public virtual IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

        // Private constructor for EF Core
        private User()
        {
            _roles = new List<UserRole>();
        }

        public User(
            string username,
            string email,
            string passwordHash,
            string passwordSalt,
            CustomerId? customerId = null)
        {
            Id = EntityId.New<UserId>();
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
            PasswordSalt = passwordSalt ?? throw new ArgumentNullException(nameof(passwordSalt));
            CustomerId = customerId;
            Status = UserStatus.Active;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            _roles = new List<UserRole>();
        }

        /// <summary>
        /// Updates the user's password.
        /// </summary>
        public void UpdatePassword(string passwordHash, string passwordSalt)
        {
            if (string.IsNullOrEmpty(passwordHash))
                throw new ArgumentNullException(nameof(passwordHash));
                
            if (string.IsNullOrEmpty(passwordSalt))
                throw new ArgumentNullException(nameof(passwordSalt));
                
            PasswordHash = passwordHash;
            PasswordSalt = passwordSalt;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the user's email.
        /// </summary>
        public void UpdateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email));
                
            Email = email;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Records a login event.
        /// </summary>
        public void RecordLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Adds a role to the user.
        /// </summary>
        public void AddRole(RoleType role)
        {
            if (!HasRole(role))
            {
                _roles.Add(new UserRole(Id, role));
                UpdatedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Removes a role from the user.
        /// </summary>
        public void RemoveRole(RoleType role)
        {
            var roleToRemove = _roles.FirstOrDefault(r => r.Role == role);
            if (roleToRemove != null)
            {
                _roles.Remove(roleToRemove);
                UpdatedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Checks if the user has a specific role.
        /// </summary>
        public bool HasRole(RoleType role)
        {
            return _roles.Any(r => r.Role == role);
        }

        /// <summary>
        /// Deactivates the user.
        /// </summary>
        public void Deactivate()
        {
            Status = UserStatus.Inactive;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Activates the user.
        /// </summary>
        public void Activate()
        {
            Status = UserStatus.Active;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Links this user to a customer.
        /// </summary>
        public void LinkToCustomer(CustomerId customerId)
        {
            if (customerId == null)
                throw new ArgumentNullException(nameof(customerId));
                
            CustomerId = customerId;
            UpdatedAt = DateTime.UtcNow;
        }
    }
} 