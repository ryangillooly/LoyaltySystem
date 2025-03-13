using LoyaltySystem.Domain.Common;
using System;

namespace LoyaltySystem.Domain.Entities
{
    /// <summary>
    /// Represents a physical or virtual location where loyalty transactions occur.
    /// </summary>
    public class Store
    {
        public StoreId Id { get; set; }
        public BrandId BrandId { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
        public GeoLocation Location { get; set; }
        public OperatingHours OperatingHours { get; set; }
        public ContactInfo ContactInfo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public virtual Brand Brand { get; set; }

        public Store() { }

        public Store(
            string brandId,
            string name,
            Address address,
            GeoLocation location,
            OperatingHours operatingHours,
            ContactInfo contactInfo)
        {
            if (string.IsNullOrWhiteSpace(brandId))
                throw new ArgumentException("BrandId cannot be empty", nameof(brandId));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Store name cannot be empty", nameof(name));

            Id = new StoreId();
            BrandId = EntityId.Parse<BrandId>(brandId);
            Name = name;
            Address = address ?? throw new ArgumentNullException(nameof(address));
            Location = location;
            OperatingHours = operatingHours;
            ContactInfo = contactInfo;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(
            string name,
            Address address,
            GeoLocation location,
            OperatingHours operatingHours,
            ContactInfo contactInfo)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Store name cannot be empty", nameof(name));

            Name = name;
            Address = address ?? throw new ArgumentNullException(nameof(address));
            Location = location;
            OperatingHours = operatingHours;
            ContactInfo = contactInfo;
            UpdatedAt = DateTime.UtcNow;
        }
    }
} 