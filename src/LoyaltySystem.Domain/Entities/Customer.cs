using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Domain.Entities
{
    public class Customer : Entity<CustomerId>
    {
        private readonly List<LoyaltyCard> _loyaltyCards;

        public Customer
        (
            string firstName,
            string lastName,
            string username,
            string email,
            string phone,
            Address? address,
            bool marketingConsent = false
        ) 
            : base(new CustomerId())
        {
            const string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, emailPattern))
                throw new ArgumentException("Invalid email format", nameof(email));
            
            Phone = phone ?? throw new ArgumentNullException(nameof(phone));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            Username = username ?? throw new ArgumentNullException(nameof(username));
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            MarketingConsent = marketingConsent;
            Address = address;
            
            _loyaltyCards = new List<LoyaltyCard>();
        }
        
        public UserId UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool MarketingConsent { get; set; }
        public DateTime? LastLoginAt { get; private set; }
        public Address? Address { get; set; }
        public List<string> Roles { get; set; } = new();
        
        public virtual IReadOnlyCollection<LoyaltyCard> LoyaltyCards => _loyaltyCards.AsReadOnly();
        
        public void Update
        (
            string firstName,
            string lastName,
            string username,
            string email,
            string phone,
            Address? address,
            bool marketingConsent
        )
        {
            const string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, emailPattern))
                throw new ArgumentException("Invalid email format", nameof(email));

            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Phone = phone ?? throw new ArgumentNullException(nameof(phone));
            Address = address;
            MarketingConsent = marketingConsent;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void AddLoyaltyCard(LoyaltyCard card)
        {
            ArgumentNullException.ThrowIfNull(card);
            _loyaltyCards.Add(card);
        }
    }
} 