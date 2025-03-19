using System;
using System.Collections.Generic;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Domain.Entities
{
    /// <summary>
    /// Represents a business organization that can own multiple brands.
    /// This is the top level entity in the hierarchy:
    /// Business > Brands > Stores > Staff
    /// </summary>
    public class Business
    {
        /// <summary>
        /// The unique identifier for the business
        /// </summary>
        public BusinessId Id { get; private set; }
        
        /// <summary>
        /// The name of the business
        /// </summary>
        public string Name { get; private set; }
        
        /// <summary>
        /// The business description
        /// </summary>
        public string Description { get; private set; }
        
        /// <summary>
        /// The tax identification number for the business
        /// </summary>
        public string TaxId { get; private set; }
        
        /// <summary>
        /// The primary contact information for the business
        /// </summary>
        public ContactInfo Contact { get; private set; }
        
        /// <summary>
        /// The primary address of the business headquarters
        /// </summary>
        public Address HeadquartersAddress { get; private set; }
        
        /// <summary>
        /// The logo URL for the business
        /// </summary>
        public string Logo { get; private set; }
        
        /// <summary>
        /// The website for the business
        /// </summary>
        public string Website { get; private set; }
        
        /// <summary>
        /// The date the business was established
        /// </summary>
        public DateTime? FoundedDate { get; private set; }
        
        /// <summary>
        /// The date and time when the business was created in the system
        /// </summary>
        public DateTime CreatedAt { get; private set; }
        
        /// <summary>
        /// The date and time when the business was last updated
        /// </summary>
        public DateTime UpdatedAt { get; private set; }
        
        /// <summary>
        /// Whether the business is currently active
        /// </summary>
        public bool IsActive { get; private set; }

        // Navigation property - collection of brands owned by this business
        public virtual ICollection<Brand> Brands { get; private set; } = new List<Brand>();

        // Private constructor for ORM
        private Business() { }

        /// <summary>
        /// Creates a new Business entity
        /// </summary>
        public Business(
            string name,
            string description,
            string taxId,
            ContactInfo contact,
            Address headquartersAddress,
            string logo = null,
            string website = null,
            DateTime? foundedDate = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Business name cannot be empty", nameof(name));

            Id = EntityId.New<BusinessId>();
            Name = name;
            Description = description;
            TaxId = taxId;
            Contact = contact ?? throw new ArgumentNullException(nameof(contact));
            HeadquartersAddress = headquartersAddress ?? throw new ArgumentNullException(nameof(headquartersAddress));
            Logo = logo;
            Website = website;
            FoundedDate = foundedDate;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            IsActive = true;
        }

        /// <summary>
        /// Updates the business information
        /// </summary>
        public void Update(
            string name,
            string description,
            string taxId,
            ContactInfo contact,
            Address headquartersAddress,
            string logo,
            string website,
            DateTime? foundedDate)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Business name cannot be empty", nameof(name));

            Name = name;
            Description = description;
            TaxId = taxId;
            Contact = contact ?? throw new ArgumentNullException(nameof(contact));
            HeadquartersAddress = headquartersAddress ?? throw new ArgumentNullException(nameof(headquartersAddress));
            Logo = logo;
            Website = website;
            FoundedDate = foundedDate;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Deactivates the business
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Activates the business
        /// </summary>
        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Adds a brand to this business
        /// </summary>
        public void AddBrand(Brand brand)
        {
            if (brand == null)
                throw new ArgumentNullException(nameof(brand));

            Brands.Add(brand);
            UpdatedAt = DateTime.UtcNow;
        }
    }
} 