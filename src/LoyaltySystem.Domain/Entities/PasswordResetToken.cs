using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Infrastructure.Entities;

public class PasswordResetToken
{
    public PasswordResetToken(
        Guid userId,
        string token,
        DateTime expiresAt,
        bool isUsed,
        Guid? id)
    {
        Id = id ?? Guid.NewGuid();
        UserId = new UserId(userId);
        Token = token;
        ExpiresAt = expiresAt;
        IsUsed = isUsed;
        CreatedAt = DateTime.UtcNow;
    }
    public Guid Id { get; set; }
    public UserId UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
}
