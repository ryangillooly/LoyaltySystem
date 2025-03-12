using System.Security.Claims;

namespace LoyaltySystem.Domain.Interfaces;

public interface IJwtService
{
    string GenerateToken(
        string userId, 
        string username, 
        string email, 
        IEnumerable<string> roles, 
        IDictionary<string, string> additionalClaims = null);
    ClaimsPrincipal ValidateToken(string token);
    bool TryParseTokenFromAuthHeader(string authHeader, out string token);
    string GenerateRefreshToken();
    Guid? GetUserIdFromToken(string token);
    string GetRoleFromToken(string token);
    bool IsTokenValid(string token);
    DateTime GetTokenExpirationTime(string token);
}