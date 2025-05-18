using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.ValueObjects;
using System;

namespace LoyaltySystem.Domain.Entities;

/// <summary>
/// Represents a business entity that offers loyalty programs to its customers.
/// </summary>
public class Brand : Entity<BrandId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Brand"/> class with the specified details for a business offering a loyalty program.
    /// </summary>
    /// <param name="name">The brand's name.</param>
    /// <param name="category">The business category of the brand.</param>
    /// <param name="logo">The logo image or path for the brand.</param>
    /// <param name="description">A description of the brand.</param>
    /// <param name="contact">Contact information for the brand.</param>
    /// <param name="address">The physical address of the brand.</param>
    /// <param name="businessId">The unique identifier for the associated business.</param>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
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

    /// <summary>
    /// Updates the brand's details and sets the updated timestamp.
    /// </summary>
    /// <param name="name">The new name of the brand.</param>
    /// <param name="category">The new category of the brand.</param>
    /// <param name="logo">The new logo URL or identifier.</param>
    /// <param name="description">The new description of the brand.</param>
    /// <param name="contact">The new contact information.</param>
    /// <param name="address">The new address.</param>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
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