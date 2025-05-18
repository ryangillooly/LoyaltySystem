using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Application.DTOs.Customers;

public class CustomerDto 
{
    public CustomerId Id { get; set; }
    public UserId UserId { get; set; } 
    public string PrefixedId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool MarketingConsent { get; set; }
    public Address? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool IsEmailConfirmed { get; set; }
    
    public static CustomerDto From(Customer customer) =>
        new ()
        {
            Id          = customer.Id,
            PrefixedId  = customer.Id.Prefix,
            UserId      = customer.UserId,
            FirstName   = customer.FirstName,
            LastName    = customer.LastName,
            Username    = customer.Username,
            Email       = customer.Email,
            Status      = customer.Status.ToString(),
            CreatedAt   = customer.CreatedAt,
            LastLoginAt = customer.LastLoginAt,
            Roles       = customer.Roles.Select(r => r).ToList()
        };
}
