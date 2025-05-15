using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.ValueObjects;
using System;

namespace LoyaltySystem.Domain.Entities;

/// <summary>
/// Represents a business entity that offers loyalty programs to its customers.
/// </summary>
public class Brand : Entity<BrandId>
{
    public Brand
    (
        string name,
        string category,
        string logo,
        string description,
        ContactInfo contact,
        Address address,
        BusinessId businessId
    ) 
    : base(new BrandId())
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Logo = logo ?? throw new ArgumentNullException(nameof(logo));
        Contact = contact ?? throw new ArgumentNullException(nameof(contact));
        Address = address ?? throw new ArgumentNullException(nameof(address));
        Category = category ?? throw new ArgumentNullException(nameof(category));
        BusinessId = businessId ?? throw new ArgumentNullException(nameof(businessId));
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }
    
    public string Name { get; private set; }
    public string Category { get; private set; }
    public string Logo { get; private set; }
    public string Description { get; private set; }
    public BusinessId BusinessId { get; private set; }
    public ContactInfo Contact { get; private set; }
    public Address Address { get; private set; }

    public void Update
    (
        string name,
        string category,
        string logo,
        string description,
        ContactInfo contact,
        Address address
    )
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Logo = logo ?? throw new ArgumentNullException(nameof(logo));
        Contact = contact ?? throw new ArgumentNullException(nameof(contact));
        Address = address ?? throw new ArgumentNullException(nameof(address));
        Category = category ?? throw new ArgumentNullException(nameof(category));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        UpdatedAt = DateTime.UtcNow;
    }
}