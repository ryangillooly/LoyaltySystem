using LoyaltySystem.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LoyaltySystem.Shared.API.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LoyaltySystem.Shared.API.Services;

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<JwtService> _logger;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public JwtService(
        IOptions<JwtSettings> jwtSettings,
        ILogger<JwtService> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _logger = logger;

        // Set up token validation parameters once for reuse
        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            ClockSkew = TimeSpan.Zero // Optionally reduce clock skew to make expiration time more accurate
        };
    }

    /// <summary>
    /// Generates a JWT token for the specified user
    /// </summary>
    public string GenerateToken(
        string userId, 
        string username, 
        string email, 
        IEnumerable<string> roles,
        IDictionary<string, string> additionalClaims = null)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            
            // Create standard claims
            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, userId),
                new (JwtRegisteredClaimNames.Sub, userId), // Subject claim
                new (JwtRegisteredClaimNames.Email, email),
                new (ClaimTypes.Email, email), // Include both formats for compatibility
                new (ClaimTypes.Name, username),
                new ("username", username), // Custom claim for username
            };
            
            // Add each role as a separate claim
            claims.AddRange(
                roles.Select(
                    role => new Claim(ClaimTypes.Role, role)));
            
            // Add additional claims
            if (additionalClaims is { })
            {
                claims.AddRange(
                    additionalClaims.Select(
                        claim => new Claim(claim.Key, claim.Value)));
            }

            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);
            
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            _logger.LogInformation("Generated JWT token for user {UserId} with roles: {Roles}", 
                userId, string.Join(", ", roles));

            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JWT token");
            throw new InvalidOperationException("Error generating JWT token", ex);
        }
    }

    /// <summary>
    /// Validates a JWT token and returns the claims principal if valid
    /// </summary>
    public ClaimsPrincipal ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out _);
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating JWT token");
            return null;
        }
    }

    /// <summary>
    /// Tries to parse the token from an Authorization header
    /// </summary>
    public bool TryParseTokenFromAuthHeader(string authHeader, out string token)
    {
        token = null;
        
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return false;
            
        token = authHeader.Substring("Bearer ".Length).Trim();
        return !string.IsNullOrEmpty(token);
    }
    
    /// <summary>
    /// Generates a cryptographically secure refresh token
    /// </summary>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Gets the user ID from a JWT token
    /// </summary>
    public Guid? GetUserIdFromToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null)
        {
            return null;
        }

        // Try to get the user ID from the sub claim
        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        
        // If sub claim wasn't found or valid, try the nameidentifier claim
        var nameIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(nameIdClaim) && Guid.TryParse(nameIdClaim, out var nameId))
        {
            return nameId;
        }

        // No valid user ID found
        _logger.LogWarning("No valid user ID found in token");
        return null;
    }

    /// <summary>
    /// Extracts the role from a token
    /// </summary>
    public string GetRoleFromToken(string token)
    {
        var principal = ValidateToken(token);
        return principal?.FindFirst(ClaimTypes.Role)?.Value;
    }

    /// <summary>
    /// Checks if a token is valid (not expired and correctly signed)
    /// </summary>
    public bool IsTokenValid(string token)
    {
        return ValidateToken(token) != null;
    }

    /// <summary>
    /// Gets the expiration time of a token
    /// </summary>
    public DateTime GetTokenExpirationTime(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return DateTime.MinValue;
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            var expClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp);
            if (expClaim == null)
            {
                return DateTime.MinValue;
            }
            
            // The exp claim is in Unix time (seconds since epoch)
            var unixTime = long.Parse(expClaim.Value);
            var expDateTime = DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime;
            
            return expDateTime;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting token expiration time");
            return DateTime.MinValue;
        }
    }
}