using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.Interfaces.Auth;

public interface ITokenService
{
    Task<string> GenerateAndStoreTokenAsync(UserId userId, VerificationTokenType type);
    Task<OperationResult> IsTokenValidAsync(VerificationTokenType type, string token);
    Task InvalidateAllTokensAsync(UserId userId, VerificationTokenType type);
    Task InvalidateTokenAsync(VerificationTokenType type, string token);
    Task MarkTokenAsUsedAsync(VerificationTokenType type, string token);
    Task<VerificationToken?> GetValidTokenAsync(VerificationTokenType type, string token);
}