using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Application.DTOs.Auth;

public class RegisterUserRequestDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public List<RoleType> Roles { get; set; } = new () { RoleType.User };
    public bool IsEmailConfirmed { get; set; }
    public bool MarketingConsent { get; set; } = false;
    public DateTime? DateOfBirth { get; set; } = null;
    public Address? Address { get; set; } = null;
}
