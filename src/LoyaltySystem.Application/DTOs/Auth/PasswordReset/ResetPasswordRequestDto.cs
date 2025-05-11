namespace LoyaltySystem.Application.DTOs.Auth.PasswordReset;

public class ResetPasswordRequestDto
{
    public string Email { get; set; }
    public string Token { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; }
}