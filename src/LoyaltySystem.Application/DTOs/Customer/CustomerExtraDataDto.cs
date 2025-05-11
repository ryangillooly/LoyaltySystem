using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Application.DTOs.Customer;

public class CustomerExtraData
{
    public Address? Address { get; set; }
    public bool? MarketingConsent { get; set; }
    public DateTime? DateOfBirth { get; set; }
}
