using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Repositories;

public interface ITokenRepository
{
    Task StoreTokenAsync(VerificationToken token);
    Task<VerificationToken?> GetValidTokenAsync(VerificationTokenType type, string token);
    Task InvalidateAllTokensAsync(UserId userId, VerificationTokenType type);
    Task InvalidateTokenAsync(VerificationTokenType type, string token);
    Task MarkTokenAsUsedAsync(VerificationTokenType type, string token);
}