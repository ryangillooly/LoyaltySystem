using LoyaltySystem.Infrastructure.Entities;

namespace LoyaltySystem.Application.DTOs.Auth.PasswordReset;

public class PasswordResetTokenDto 
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }

    public PasswordResetToken ToDomainModel() =>
        new (UserId, Token, ExpiresAt, IsUsed, Id);
}
