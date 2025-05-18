using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.DTOs.Auth;

public class LoginRequestDto : AuthDto
{
    public string Password { get; set; } = string.Empty;
}