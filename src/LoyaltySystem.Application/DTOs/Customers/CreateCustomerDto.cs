using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.Application.DTOs.Customers;

public class CreateCustomerDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool MarketingConsent { get; set; }
    public DateTime? DateOfBirth { get; set; } = null;
    public Address? Address { get; set; }
}