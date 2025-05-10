using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.DTOs.AuthDtos;

public class LoginRequestDto 
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Identifier => !string.IsNullOrEmpty(Email) ? Email : UserName;
    public LoginIdentifierType IdentifierType => !string.IsNullOrEmpty(Email) ? LoginIdentifierType.Email : LoginIdentifierType.Username;
}