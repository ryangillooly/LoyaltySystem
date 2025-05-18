using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Entities
{
    public class User : Entity<UserId> 
    {
        private readonly List<UserRole> _roles = new();
        
        public CustomerId? CustomerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserStatus Status { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();
        public bool IsEmailConfirmed { get; set; }

        public User() : base(new UserId()) { }
        
        public User
        (
            string firstName,
            string lastName,
            string userName,
            string email,
            string passwordHash,
            string phone,
            CustomerId? customerId = null,
            UserId? id = null
        ) 
        : base(id ?? new UserId())
        {
            CustomerId   = customerId;
            FirstName    = firstName    ?? throw new ArgumentNullException(nameof(firstName));
            LastName     = lastName     ?? throw new ArgumentNullException(nameof(lastName));
            Username     = userName     ?? throw new ArgumentNullException(nameof(userName));
            Email        = email        ?? throw new ArgumentNullException(nameof(email));
            Phone        = phone        ?? throw new ArgumentNullException(nameof(phone));
            PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
            Status       = UserStatus.Active;
            
            _roles = new List<UserRole> { new (Id, RoleType.User) };
        }
        
        public void UpdateEmail(string email)
        {
            ArgumentNullException.ThrowIfNull(email, nameof(email));
                
            Email = email;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void UpdateUserName(string userName)
        {
            ArgumentNullException.ThrowIfNull(userName, nameof(userName));
                
            Username = userName;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void UpdatePassword(string passwordHash)
        {
            ArgumentNullException.ThrowIfNull(passwordHash, nameof(passwordHash));
                
            PasswordHash = passwordHash;
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
    }
} 