using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Application.DTOs.Customer;

public class CustomerProfileDto : ProfileDto 
{
    public string Phone { get; set; } = string.Empty;
    public bool MarketingConsent { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Address? Address { get; set; }
}
