using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.Interfaces.Auth;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Repositories;

namespace LoyaltySystem.Application.Services.Auth;

public class TokenService : ITokenService
{
    private readonly ITokenRepository _repository;

    /// <summary>
        /// Initializes a new instance of the <see cref="TokenService"/> class with the specified token repository.
        /// </summary>
        public TokenService(ITokenRepository repository) =>
        _repository = repository;

    /// <summary>
    /// Generates a new secure verification token for the specified user and type, invalidates all previous tokens of that type for the user, stores the new token, and returns it.
    /// </summary>
    /// <param name="userId">The identifier of the user for whom the token is generated.</param>
    /// <param name="type">The type of verification token to generate.</param>
    /// <returns>The newly generated verification token string.</returns>
    public async Task<string> GenerateAndStoreTokenAsync(UserId userId, VerificationTokenType type)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(type);
        
        await _repository.InvalidateAllTokensAsync(userId, type);
        var token = SecurityUtils.GenerateSecureToken();
        
        await _repository.StoreTokenAsync(new VerificationToken(userId, token, isValid: true, type));

        return token;
    }
    
    /// <summary>
    /// Checks whether the specified verification token is valid and not expired.
    /// </summary>
    /// <param name="type">The type of verification token to validate.</param>
    /// <param name="token">The token string to validate.</param>
    /// <returns>An <see cref="OperationResult"/> indicating success if the token is valid and unexpired, or failure with an error message if invalid or expired.</returns>
    public async Task<OperationResult> IsTokenValidAsync(VerificationTokenType type, string token)
    {
        var record = await _repository.GetValidTokenAsync(type, token);
        if (record is not { IsValid: true } || DateTime.UtcNow >= record.ExpiresAt)
            return OperationResult.FailureResult("Invalid or expired verification token.");
        
        return OperationResult.SuccessResult();
    }

    /// <summary>
        /// Marks the specified verification token as used, preventing further use.
        /// </summary>
        /// <param name="type">The type of the verification token.</param>
        /// <param name="token">The token string to mark as used.</param>
        public async Task MarkTokenAsUsedAsync(VerificationTokenType type, string token) =>
        await _repository.MarkTokenAsUsedAsync(type, token);

    /// <summary>
        /// Invalidates all tokens for the specified user and token type.
        /// </summary>
        public async Task InvalidateAllTokensAsync(UserId userId, VerificationTokenType type) =>
        await _repository.InvalidateAllTokensAsync(userId, type);
    
    /// <summary>
        /// Invalidates a specific verification token of the given type.
        /// </summary>
        /// <param name="type">The type of the verification token to invalidate.</param>
        /// <param name="token">The token string to invalidate.</param>
        public async Task InvalidateTokenAsync(VerificationTokenType type, string token) =>
        await _repository.InvalidateTokenAsync(type, token);

    /// <summary>
        /// Retrieves a valid verification token of the specified type and token string if it exists and is not expired.
        /// </summary>
        /// <param name="type">The type of verification token to retrieve.</param>
        /// <param name="token">The token string to match.</param>
        /// <returns>The valid <see cref="VerificationToken"/> if found; otherwise, null.</returns>
        public async Task<VerificationToken?> GetValidTokenAsync(VerificationTokenType type, string token) =>
        await _repository.GetValidTokenAsync(type, token);
}