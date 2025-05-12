using LoyaltySystem.Domain.Entities;

namespace LoyaltySystem.Application.DTOs.Auth.EmailVerification;

public class EmailConfirmationTokenDto 
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public EmailConfirmationToken ToDomainModel() =>
        new (UserId, Token, ExpiresAt, IsUsed, Id);
}
