using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace LoyaltySystem.Domain.Entities
{
    public class Customer : Entity<CustomerId>
    {
        private readonly List<LoyaltyCard> _loyaltyCards;

        public Customer() : base(new CustomerId())
        {
            _loyaltyCards = new List<LoyaltyCard>();
        }

        public Customer
        (
            string firstName,
            string lastName,
            string username,
            string email,
            string phone,
            Address? address,
            bool marketingConsent = false,
            CustomerId? customerId = null) : base(customerId ?? new CustomerId())
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("FirstName cannot be empty", nameof(firstName));
            
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("LastName cannot be empty", nameof(lastName));

            if (!string.IsNullOrWhiteSpace(email))
            {
                var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(email, emailPattern))
                    throw new ArgumentException("Invalid email format", nameof(email));
            }
            
            FirstName = firstName;
            LastName = lastName;
            UserName = username;
            Email = email;
            Phone = phone;
            MarketingConsent = marketingConsent;
            Address = address;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            _loyaltyCards = new List<LoyaltyCard>();
        }
        
        /// <summary>
        /// The generated human-readable prefixed ID (e.g., cus_xxxx). 
        /// Should be generated and assigned just before saving the entity for the first time.
        /// </summary>
        public string PrefixedId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool MarketingConsent { get; set; }
        public Address? Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        public virtual IReadOnlyCollection<LoyaltyCard> LoyaltyCards => _loyaltyCards.AsReadOnly();

        
        
        public void Update(
            string firstName,
            string lastName,
            string username,
            string email,
            string phone,
            Address? address,
            bool marketingConsent)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("FirstName cannot be empty", nameof(firstName));
            
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("LastName cannot be empty", nameof(lastName));

            if (!string.IsNullOrWhiteSpace(email))
            {
                var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(email, emailPattern))
                    throw new ArgumentException("Invalid email format", nameof(email));
            }

            FirstName = firstName;
            LastName = lastName;
            UserName = username;
            Email = email;
            Address = address;
            Phone = phone;
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