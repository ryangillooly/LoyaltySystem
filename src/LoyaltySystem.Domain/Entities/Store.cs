using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Domain.Entities;

/// <summary>
/// Represents a physical or virtual location where loyalty transactions occur.
/// </summary>
public class Store : Entity<StoreId>
{
    public BrandId BrandId { get; set; }
    public string Name { get; set; }
    public Address Address { get; set; }
    public GeoLocation Location { get; set; }
    public OperatingHours OperatingHours { get; set; }
    public ContactInfo ContactInfo { get; set; }
    public virtual Brand Brand { get; set; }

    public Store() : base(new StoreId()) { }
    public Store
    (
        BrandId brandId,
        string name,
        Address address,
        GeoLocation location,
        OperatingHours operatingHours,
        ContactInfo contactInfo
    )
    : base(new StoreId())
    {
        Name           = name ?? throw new ArgumentNullException(nameof(name));
        BrandId        = brandId ?? throw new ArgumentNullException(nameof(brandId));
        Address        = address ?? throw new ArgumentNullException(nameof(address));
        Location       = location ?? throw new ArgumentNullException(nameof(location));
        ContactInfo    = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
        OperatingHours = operatingHours ?? throw new ArgumentNullException(nameof(operatingHours));
    }

    public void Update
    (
        string name,
        Address address,
        GeoLocation location,
        OperatingHours operatingHours,
        ContactInfo contactInfo
    )
    {
        UpdatedAt      = DateTime.UtcNow;
        Name           = name ?? throw new ArgumentNullException(nameof(name));
        Address        = address ?? throw new ArgumentNullException(nameof(address));
        Location       = location ?? throw new ArgumentNullException(nameof(location));
        ContactInfo    = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
        OperatingHours = operatingHours ?? throw new ArgumentNullException(nameof(operatingHours));
    }
}