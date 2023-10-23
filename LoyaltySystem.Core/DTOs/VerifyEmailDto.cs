namespace LoyaltySystem.Core.DTOs;

public class VerifyEmailDto
{
    public VerifyEmailDto(Guid userId, Guid token)
    {
        UserId = userId;
        Token = token;
    }
    
    public Guid UserId { get; set; }
    public Guid Token { get; set; }
}