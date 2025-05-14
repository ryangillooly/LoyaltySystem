using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Application.DTOs.Customers;

public class CustomerExtraData 
{
    public bool MarketingConsent { get; set; } = false;
    public DateTime? DateOfBirth { get; set; } = null;
    public Address? Address { get; set; } = null;
}
