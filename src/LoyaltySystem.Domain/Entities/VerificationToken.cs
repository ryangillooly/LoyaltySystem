using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Entities;

public class VerificationToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public UserId UserId { get; set; }
    public string Token { get; set; }
    public VerificationTokenType Type { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsValid { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Optionally: public string? Data { get; set; } // For extra info (JSON)
}
