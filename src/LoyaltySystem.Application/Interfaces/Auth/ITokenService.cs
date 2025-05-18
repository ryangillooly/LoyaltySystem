using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.Interfaces.Auth;

public interface ITokenService
{
    /// <summary>
/// Generates a new verification token for the specified user and token type, stores it, and returns the token string.
/// </summary>
/// <param name="userId">The identifier of the user for whom the token is generated.</param>
/// <param name="type">The type of verification token to generate.</param>
/// <returns>The generated token string.</returns>
Task<string> GenerateAndStoreTokenAsync(UserId userId, VerificationTokenType type);
    /// <summary>
/// Checks whether a verification token of the specified type is valid.
/// </summary>
/// <param name="type">The type of verification token to validate.</param>
/// <param name="token">The token string to validate.</param>
/// <returns>An <see cref="OperationResult"/> indicating whether the token is valid.</returns>
Task<OperationResult> IsTokenValidAsync(VerificationTokenType type, string token);
    /// <summary>
/// Invalidates all tokens of the specified type for the given user.
/// </summary>
/// <param name="userId">The identifier of the user whose tokens will be invalidated.</param>
/// <param name="type">The type of tokens to invalidate.</param>
Task InvalidateAllTokensAsync(UserId userId, VerificationTokenType type);
    /// <summary>
/// Invalidates a specific verification token of the given type.
/// </summary>
/// <param name="type">The type of verification token to invalidate.</param>
/// <param name="token">The token string to invalidate.</param>
Task InvalidateTokenAsync(VerificationTokenType type, string token);
    /// <summary>
/// Marks the specified verification token as used to prevent further use.
/// </summary>
/// <param name="type">The type of the verification token.</param>
/// <param name="token">The token string to mark as used.</param>
Task MarkTokenAsUsedAsync(VerificationTokenType type, string token);
    /// <summary>
/// Retrieves a valid verification token entity matching the specified type and token string, or returns null if no valid token is found.
/// </summary>
/// <param name="type">The type of verification token to search for.</param>
/// <param name="token">The token string to match.</param>
/// <returns>The valid <see cref="VerificationToken"/> if found; otherwise, null.</returns>
Task<VerificationToken?> GetValidTokenAsync(VerificationTokenType type, string token);
}