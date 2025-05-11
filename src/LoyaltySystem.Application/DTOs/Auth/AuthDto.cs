using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.DTOs.Auth;

public class AuthDto 
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Identifier => !string.IsNullOrEmpty(Email) ? Email : UserName;
    public AuthIdentifierType IdentifierType => Identifier == Email 
        ? AuthIdentifierType.Email 
        : AuthIdentifierType.Username;
}
