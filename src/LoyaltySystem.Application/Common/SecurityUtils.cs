using System.Security.Cryptography;

namespace LoyaltySystem.Application.Common;

public static class SecurityUtils 
{
    public static string GenerateSecureToken(int size = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(size);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", ""); // Make it URL-safe
    }    
}
