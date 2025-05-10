using LoyaltySystem.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.Application.DTOs.Customer;

public class CreateCustomerDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string UserName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string Phone { get; set; } = string.Empty;
    public bool MarketingConsent { get; set; }
    public DateTime? DateOfBirth { get; set; } = null;
    public Address? Address { get; set; }
}