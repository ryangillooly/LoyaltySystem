using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.ValueObjects;
using System;

namespace LoyaltySystem.Domain.Entities
{
    /// <summary>
    /// Represents a business entity that offers loyalty programs to its customers.
    /// </summary>
    public class Brand
    {
        public BrandId Id { get; private set; }
        public string Name { get; private set; }
        public string Category { get; private set; }
        public string Logo { get; private set; }
        public string Description { get; private set; }
        public BusinessId BusinessId { get; private set; }
        public ContactInfo Contact { get; private set; }
        public Address Address { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // Private constructor for EF Core
        private Brand() { }

        public Brand(
            string name,
            string category,
            string logo,
            string description,
            ContactInfo contact,
            Address address,
            BusinessId businessId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Brand name cannot be empty", nameof(name));

            Id = new BrandId();
            Name = name;
            Category = category;
            Logo = logo;
            Description = description;
            Contact = contact ?? throw new ArgumentNullException(nameof(contact));
            Address = address;
            BusinessId = businessId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(
            string name,
            string category,
            string logo,
            string description,
            ContactInfo contact,
            Address address)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Brand name cannot be empty", nameof(name));

            Name = name;
            Category = category;
            Logo = logo;
            Description = description;
            Contact = contact ?? throw new ArgumentNullException(nameof(contact));
            Address = address;
            UpdatedAt = DateTime.UtcNow;
        }
        
        // Internal methods for Dapper to use when materializing objects
        public void SetContactInfo(ContactInfo contact)
        {
            Contact = contact;
        }
        
        public void SetAddress(Address address)
        {
            Address = address;
        }
    }
} 