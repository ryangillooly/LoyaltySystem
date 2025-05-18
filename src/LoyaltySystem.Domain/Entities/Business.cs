using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Domain.Entities;

public class Business : Entity<BusinessId> 
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Business"/> class with the specified details.
    /// </summary>
    /// <param name="name">The business name.</param>
    /// <param name="description">A description of the business.</param>
    /// <param name="taxId">The tax identification number for the business.</param>
    /// <param name="contact">Contact information for the business. Cannot be null.</param>
    /// <param name="headquartersAddress">The headquarters address of the business. Cannot be null.</param>
    /// <param name="logo">An optional logo URL or path.</param>
    /// <param name="website">An optional website URL.</param>
    /// <param name="foundedDate">The optional date the business was founded.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="contact"/> or <paramref name="headquartersAddress"/> is null.</exception>
    public Business
    (
        string name,
        string description,
        string taxId,
        ContactInfo contact,
        Address headquartersAddress,
        string logo = null,
        string website = null,
        DateTime? foundedDate = null
    ) 
    : base(new BusinessId())
    {
        Name = name;
        Description = description;
        TaxId = taxId;
        Contact = contact ?? throw new ArgumentNullException(nameof(contact));
        HeadquartersAddress = headquartersAddress ?? throw new ArgumentNullException(nameof(headquartersAddress));
        Logo = logo;
        Website = website;
        FoundedDate = foundedDate;
        IsActive = true;
    }
    
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string TaxId { get; private set; }
    public ContactInfo Contact { get; private set; }
    public Address HeadquartersAddress { get; private set; }
    public string Logo { get; private set; }
    public string Website { get; private set; }
    public DateTime? FoundedDate { get; private set; }
    public bool IsActive { get; private set; }
    public virtual ICollection<Brand> Brands { get; private set; } = new List<Brand>();

    /// <summary>
    /// Updates the business's details with new values and refreshes the update timestamp.
    /// </summary>
    /// <param name="name">The new name of the business.</param>
    /// <param name="description">The new description of the business.</param>
    /// <param name="taxId">The new tax identification number.</param>
    /// <param name="contact">The new contact information.</param>
    /// <param name="headquartersAddress">The new headquarters address.</param>
    /// <param name="logo">The new logo URL or path.</param>
    /// <param name="website">The new website URL.</param>
    /// <param name="foundedDate">The new founding date, or null if unknown.</param>
    /// <exception cref="ArgumentNullException">Thrown if any parameter except <paramref name="foundedDate"/> is null.</exception>
    public void Update
    (
        string name,
        string description,
        string taxId,
        ContactInfo contact,
        Address headquartersAddress,
        string logo,
        string website,
        DateTime? foundedDate
    )
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Logo = logo ?? throw new ArgumentNullException(nameof(logo));
        TaxId = taxId ?? throw new ArgumentNullException(nameof(taxId));
        Contact = contact ?? throw new ArgumentNullException(nameof(contact));
        Website = website ?? throw new ArgumentNullException(nameof(website));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        HeadquartersAddress = headquartersAddress ?? throw new ArgumentNullException(nameof(headquartersAddress));
        FoundedDate = foundedDate;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Marks the business as inactive and updates the last modified timestamp.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Marks the business as active and updates the last modified timestamp.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Adds a brand to the business's collection of brands.
    /// </summary>
    /// <param name="brand">The brand to associate with the business.</param>
    public void AddBrand(Brand brand)
    {
        ArgumentNullException.ThrowIfNull(brand);

        Brands.Add(brand);
        UpdatedAt = DateTime.UtcNow;
    }
}