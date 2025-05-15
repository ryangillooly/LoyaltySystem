using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Domain.Entities;

public class Business : Entity<BusinessId> 
{
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
    
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void AddBrand(Brand brand)
    {
        ArgumentNullException.ThrowIfNull(brand);

        Brands.Add(brand);
        UpdatedAt = DateTime.UtcNow;
    }
}