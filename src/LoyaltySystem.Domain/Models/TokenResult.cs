using System;

namespace LoyaltySystem.Domain.Models // Or LoyaltySystem.Domain.Interfaces if preferred
{
    /// <summary>
    /// Represents the result of a token generation operation, including the token and its metadata.
    /// </summary>
    public class TokenResult
    {
        /// <summary>
        /// The generated JWT access token string.
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// The type of the token (e.g., "Bearer").
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// The lifetime of the access token in seconds.
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Optional refresh token (not implemented in this version).
        /// </summary>
        public string? RefreshToken { get; set; }

        // Optional: Add constructor for easier creation
        public TokenResult(string accessToken, int expiresIn, string tokenType = "Bearer", string? refreshToken = null)
        {
            AccessToken = accessToken;
            ExpiresIn = expiresIn;
            TokenType = tokenType;
            RefreshToken = refreshToken;
        }
    }
} 