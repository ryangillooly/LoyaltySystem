using LoyaltySystem.Domain.Common;
using System;
using System.Collections.Generic;

namespace LoyaltySystem.Domain.Entities
{
    /// <summary>
    /// Represents a customer who participates in loyalty programs.
    /// </summary>
    public class Customer
    {
        private readonly List<LoyaltyCard> _loyaltyCards;

        public CustomerId Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Phone { get; private set; }
        public bool MarketingConsent { get; private set; }
        public DateTime JoinedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // Collection navigation property
        public virtual IReadOnlyCollection<LoyaltyCard> LoyaltyCards => _loyaltyCards.AsReadOnly();

        // Private constructor for EF Core
        private Customer() 
        {
            _loyaltyCards = new List<LoyaltyCard>();
        }

        public Customer(
            string name,
            string email,
            string phone,
            CustomerId? customerId = null,
            bool marketingConsent = false)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Customer name cannot be empty", nameof(name));

            if (!string.IsNullOrWhiteSpace(email) && !email.Contains("@"))
                throw new ArgumentException("Invalid email format", nameof(email));

            Id = customerId ?? new CustomerId();
            Name = name;
            Email = email;
            Phone = phone;
            MarketingConsent = marketingConsent;
            JoinedAt = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            _loyaltyCards = new List<LoyaltyCard>();
        }

        /// <summary>
        /// Updates the customer's information.
        /// </summary>
        public void Update(
            string name,
            string email,
            string phone,
            bool marketingConsent)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Customer name cannot be empty", nameof(name));

            if (!string.IsNullOrWhiteSpace(email) && !email.Contains("@"))
                throw new ArgumentException("Invalid email format", nameof(email));

            Name = name;
            Email = email;
            Phone = phone;
            MarketingConsent = marketingConsent;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Records a login event for the customer.
        /// </summary>
        public void RecordLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Adds a loyalty card to the customer.
        /// </summary>
        public void AddLoyaltyCard(LoyaltyCard card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));
                
            _loyaltyCards.Add(card);
        }
    }
} 