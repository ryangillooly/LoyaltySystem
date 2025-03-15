using System;
using System.Collections.Generic;
using System.Linq;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Entities
{
    public class User : Entity<UserId>
    {
        private readonly List<UserRole> _roles = new();
        
        public User() : base(new UserId()) { }
        
        public User(
            string firstName,
            string lastName,
            string email,
            string passwordHash,
            string passwordSalt) : base(new UserId())
        {
            FirstName = FirstName;
            LastName = lastName;
            Email = email;
            PasswordHash = passwordHash;
            PasswordSalt = passwordSalt;
            Status = UserStatus.Active;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            LastLoginAt = null;
            CustomerId = new CustomerId();
        }
        
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public string PasswordSalt { get; private set; }
        public UserStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public CustomerId? CustomerId { get; private set; }
        public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();
        
        public void UpdateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.", nameof(email));
                
            Email = email;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void UpdatePassword(string passwordHash, string passwordSalt)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));
                
            if (string.IsNullOrWhiteSpace(passwordSalt))
                throw new ArgumentException("Password salt cannot be empty.", nameof(passwordSalt));
                
            PasswordHash = passwordHash;
            PasswordSalt = passwordSalt;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void RecordLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void LinkToCustomer(string customerId)
        {
            CustomerId = EntityId.Parse<CustomerId>(customerId);
            UpdatedAt = DateTime.UtcNow;
            AddRole(RoleType.Customer);
        }
        
        public void AddRole(RoleType role)
        {
            if (!HasRole(role))
                _roles.Add(new UserRole(Id, role));
        }
        
        public void RemoveRole(RoleType role)
        {
            var roleToRemove = _roles.Find(r => r.Role == role);
            if (roleToRemove is { })
                _roles.Remove(roleToRemove);
        }
        
        public bool HasRole(RoleType role) => _roles.Exists(r => r.Role == role);
        
        public void Activate()
        {
            Status = UserStatus.Active;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void Deactivate()
        {
            Status = UserStatus.Inactive;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void Lock()
        {
            Status = UserStatus.Locked;
            UpdatedAt = DateTime.UtcNow;
        }
    }
} 