using System;

namespace LoyaltySystem.Domain.Entities
{
    /// <summary>
    /// Represents a physical or virtual location where loyalty transactions occur.
    /// </summary>
    public class Store
    {
        public Guid Id { get; private set; }
        public Guid BrandId { get; private set; }
        public string Name { get; private set; }
        public Address Address { get; private set; }
        public GeoLocation Location { get; private set; }
        public OperatingHours Hours { get; private set; }
        public string ContactInfo { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // Navigation property
        public virtual Brand Brand { get; private set; }

        // Private constructor for EF Core
        private Store() { }

        public Store(
            Guid brandId,
            string name,
            Address address,
            GeoLocation location,
            OperatingHours hours,
            string contactInfo)
        {
            if (brandId == Guid.Empty)
                throw new ArgumentException("BrandId cannot be empty", nameof(brandId));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Store name cannot be empty", nameof(name));

            Id = Guid.NewGuid();
            BrandId = brandId;
            Name = name;
            Address = address ?? throw new ArgumentNullException(nameof(address));
            Location = location;
            Hours = hours;
            ContactInfo = contactInfo;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(
            string name,
            Address address,
            GeoLocation location,
            OperatingHours hours,
            string contactInfo)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Store name cannot be empty", nameof(name));

            Name = name;
            Address = address ?? throw new ArgumentNullException(nameof(address));
            Location = location;
            Hours = hours;
            ContactInfo = contactInfo;
            UpdatedAt = DateTime.UtcNow;
        }
        
        // Internal methods for Dapper to use when materializing objects
        public void SetAddress(Address address)
        {
            Address = address;
        }

        public void SetLocation(GeoLocation location)
        {
            Location = location;
        }
        
        internal void SetHours(OperatingHours hours)
        {
            Hours = hours;
        }
    }
} 