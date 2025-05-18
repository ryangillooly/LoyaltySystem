using System.Security.Cryptography;

namespace LoyaltySystem.Application.Common;

public static class SecurityUtils 
{
    /// <summary>
    /// Generates a cryptographically secure, URL-safe random token as a string.
    /// </summary>
    /// <param name="size">The number of random bytes to use for token generation. Defaults to 32.</param>
    /// <returns>A URL-safe Base64-encoded string representing the random token.</returns>
    public static string GenerateSecureToken(int size = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(size);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", ""); // Make it URL-safe
    }    
}
