using LoyaltySystem.Domain.Entities;

namespace LoyaltySystem.Application.DTOs;

public class UserDto
{
    public Guid Id { get; set; } // Changed type from string to Guid
    public string PrefixedId { get; set; } = string.Empty; // Added this property
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty; 
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool IsEmailConfirmed { get; set; }
}