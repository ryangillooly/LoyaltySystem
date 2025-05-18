using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Domain.Entities
{
    public class Customer : Entity<CustomerId>
    {
        private readonly List<LoyaltyCard> _loyaltyCards;

        /// <summary>
        /// Initializes a new Customer with the specified personal details, contact information, and optional address and marketing consent.
        /// </summary>
        /// <param name="firstName">The customer's first name. Cannot be null.</param>
        /// <param name="lastName">The customer's last name. Cannot be null.</param>
        /// <param name="username">The customer's username. Cannot be null.</param>
        /// <param name="email">The customer's email address. Must be a valid email format and cannot be null.</param>
        /// <param name="phone">The customer's phone number. Cannot be null.</param>
        /// <param name="address">The customer's address, or null if not provided.</param>
        /// <param name="marketingConsent">Indicates whether the customer has given marketing consent. Defaults to false.</param>
        /// <exception cref="ArgumentNullException">Thrown if any required parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the email format is invalid.</exception>
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
        
        /// <summary>
        /// Updates the customer's personal and contact information, address, and marketing consent status.
        /// </summary>
        /// <param name="firstName">The customer's first name. Must not be null.</param>
        /// <param name="lastName">The customer's last name. Must not be null.</param>
        /// <param name="username">The customer's username. Must not be null.</param>
        /// <param name="email">The customer's email address. Must be a valid email format and not null.</param>
        /// <param name="phone">The customer's phone number. Must not be null.</param>
        /// <param name="address">The customer's address, or null if not provided.</param>
        /// <param name="marketingConsent">Indicates whether the customer has given marketing consent.</param>
        /// <exception cref="ArgumentNullException">Thrown if any required parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the email format is invalid.</exception>
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
        
        /// <summary>
        /// Associates a loyalty card with the customer.
        /// </summary>
        /// <param name="card">The loyalty card to add. Must not be null.</param>
        public void AddLoyaltyCard(LoyaltyCard card)
        {
            ArgumentNullException.ThrowIfNull(card);
            _loyaltyCards.Add(card);
        }
    }
} 