using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.DTOs.Auth;

public class RegisterUserResponseDto 
{
    public UserId? Id { get; set; } = null;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public CustomerId? CustomerId { get; set; } = null; 
    public string Phone { get; set; } = string.Empty;
    public List<RoleType> Roles { get; set; } = new () { RoleType.User };
    public bool IsEmailConfirmed { get; set; }
    
    public static RegisterUserResponseDto From(User user) =>
        new ()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.Username,
            Email = user.Email,
            Phone = user.Phone,
            Status = user.Status.ToString(),
            CustomerId = user.CustomerId,
            Roles = user.Roles.Select(r => r.Role).ToList()
        };
}