namespace LoyaltySystem.Application.DTOs.Auth.Social;

public class SocialAuthResponseDto
{
    public string Token { get; set; }
    public InternalUserDto InternalUser { get; set; }
    public bool IsNewUser { get; set; }
    public string SocialId { get; set; }
    public string SocialEmail { get; set; }
}