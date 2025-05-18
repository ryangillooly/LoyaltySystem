using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Application.DTOs.Customers;

public class RegisterCustomerRequestDto : RegisterUserRequestDto
{
    public bool MarketingConsent { get; set; } = false;
    public DateTime? DateOfBirth { get; set; } = null;
    public Address? Address { get; set; } = null;
}
