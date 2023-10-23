namespace LoyaltySystem.Core.DTOs;

public class VerifyEmailDto
{
    public Guid UserId { get; set; }
    public Guid Token { get; set; }
}