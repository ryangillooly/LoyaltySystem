using System;
using System.Collections.Generic;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Domain.Entities;

public class Business : Entity<BusinessId> 
{
    public Business() : base(new BusinessId()) { }
    
    public Business
    (
        string name,
        string description,
        string taxId,
        ContactInfo contact,
        Address headquartersAddress,
        string logo = null,
        string website = null,
        DateTime? foundedDate = null) : base(new BusinessId())
    {
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
    
    public BusinessId Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string TaxId { get; private set; }
    public ContactInfo Contact { get; private set; }
    public Address HeadquartersAddress { get; private set; }
    public string Logo { get; private set; }
    public string Website { get; private set; }
    public DateTime? FoundedDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation property - collection of brands owned by this business
    public virtual ICollection<Brand> Brands { get; private set; } = new List<Brand>();

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