using LoyaltySystem.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.Application.DTOs.Customers;

public class UpdateCustomerDto 
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
    
    [Phone]
    public string Phone { get; set; } = string.Empty;
    
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    public bool MarketingConsent { get; set; }
    public DateTime? DateOfBirth { get; set; } = null;
    
    public Address? Address { get; set; } = null;
    public bool IsActive { get; set; } = true;
}