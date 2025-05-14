using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using Customer = LoyaltySystem.Domain.Entities.Customer;

namespace LoyaltySystem.Application.DTOs;

public class UserDto
{
    public UserId Id { get; set; } // Changed type from string to Guid
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty; 
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public IReadOnlyCollection<string> Roles { get; set; } = new List<string>();
    public bool IsEmailConfirmed { get; set; }
    
    public static UserDto From(User user) =>
        new ()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.UserName,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            Status = user.Status.ToString(),
            CustomerId = user.CustomerId?.ToString() ?? string.Empty,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = user.Roles.Select(r => r.Role.ToString()).ToList()
        };
}