using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Entities
{
    public class User : Entity<UserId>
    {
        private readonly List<UserRole> _roles = new();
        
        public User() : base(new UserId()) { }
        
        public User
        (
            string firstName,
            string lastName,
            string userName,
            string email,
            string passwordHash,
            string passwordSalt,
            CustomerId? customerId) : base(new UserId())
        {
            ArgumentNullException.ThrowIfNull(firstName);
            ArgumentNullException.ThrowIfNull(lastName);
            ArgumentNullException.ThrowIfNull(userName);
            ArgumentNullException.ThrowIfNull(email);
            ArgumentNullException.ThrowIfNull(passwordHash);
            ArgumentNullException.ThrowIfNull(passwordSalt);
            
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            UserName = userName;
            PasswordHash = passwordHash;
            PasswordSalt = passwordSalt;
            Status = UserStatus.Active;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            LastLoginAt = null;
            CustomerId = customerId ?? new CustomerId();
        }
        
        public string PrefixedId { get; set; } = string.Empty;
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string UserName { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public string PasswordSalt { get; private set; }
        public UserStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public CustomerId? CustomerId { get; set; }
        public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();
        
        public void UpdateEmail(string email)
        {
            ArgumentNullException.ThrowIfNull(email, nameof(email));
                
            Email = email;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void UpdateUserName(string userName)
        {
            ArgumentNullException.ThrowIfNull(userName, nameof(userName));
                
            UserName = userName;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void UpdatePassword(string passwordHash, string passwordSalt)
        {
            ArgumentNullException.ThrowIfNull(passwordHash, nameof(passwordHash));
            ArgumentNullException.ThrowIfNull(passwordSalt, nameof(passwordSalt));
                
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
            ArgumentNullException.ThrowIfNull(customerId, nameof(customerId));
            
            CustomerId = EntityId.Parse<CustomerId>(customerId);
            UpdatedAt = DateTime.UtcNow;
            AddRole(RoleType.Customer);
        }
        
        public void AddRole(RoleType role)
        {
            ArgumentNullException.ThrowIfNull(role, nameof(role));
            
            if (!HasRole(role))
                _roles.Add(new UserRole(Id, role));
        }
        
        public void RemoveRole(RoleType role)
        {
            ArgumentNullException.ThrowIfNull(role, nameof(role));
            
            var roleToRemove = _roles.Find(r => r.Role == role);
            if (roleToRemove is { })
                _roles.Remove(roleToRemove);
        }

        public bool HasRole(RoleType role)
        {
            ArgumentNullException.ThrowIfNull(role, nameof(role));
            return _roles.Exists(r => r.Role == role);
        }

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