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
    
    public static UserDto From(User user) =>
        new ()
        {
            Id = user.Id.Value,
            PrefixedId = user.Id.ToString(),
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.UserName,
            Email = user.Email,
            Status = user.Status.ToString(),
            CustomerId = user.CustomerId?.ToString() ?? string.Empty,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = user.Roles.Select(r => r.Role.ToString()).ToList()
        };
}