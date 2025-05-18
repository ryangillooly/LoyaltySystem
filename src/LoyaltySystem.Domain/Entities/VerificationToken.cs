using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Entities;

public class VerificationToken : Entity<VerificationTokenId>
{
    public VerificationToken() : base(new VerificationTokenId()) { }
    public VerificationToken
    (
        UserId userId,
        string token,
        bool? isValid,
        VerificationTokenType? type
    )
    : base(new VerificationTokenId())
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Token = token ?? throw new ArgumentNullException(nameof(token));
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        IsValid = isValid ?? throw new ArgumentNullException(nameof(isValid));
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = CalculateExpiry(Type.Value, CreatedAt);
    }
    
    public UserId UserId { get; set; }
    public string Token { get; set; }
    public VerificationTokenType? Type { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool? IsValid { get; set; }
    
    private static DateTime CalculateExpiry(VerificationTokenType type, DateTime createdAt) =>
        type is VerificationTokenType.PasswordReset
            ? createdAt.AddMinutes(30)
            : createdAt.AddDays(7);
}
