using LoyaltySystem.Domain.Entities;
using System.Security.Claims;
using LoyaltySystem.Domain.Models;

namespace LoyaltySystem.Domain.Interfaces;

public interface IJwtService
{
    /// <summary>
    /// Generates a JWT token based on a collection of claims.
    /// </summary>
    /// <param name="claims">The claims to include in the token.</param>
    /// <returns>A TokenResult object containing the access token and its metadata.</returns>
    TokenResult GenerateToken(IEnumerable<Claim> claims);
    
    ClaimsPrincipal? ValidateToken(string token);
    bool TryParseTokenFromAuthHeader(string authHeader, out string token);
    string GenerateRefreshToken();
    Guid? GetUserIdFromToken(string token);
    string GetRoleFromToken(string token);
    bool IsTokenValid(string token);
    public TokenResult GenerateTokenResult(User user);
    DateTime GetTokenExpirationTime(string token);
}