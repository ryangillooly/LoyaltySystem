namespace LoyaltySystem.Application.DTOs.Auth.PasswordReset;

public class ResetPasswordRequestDto : AuthDto
{
    public string Token { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; }
}