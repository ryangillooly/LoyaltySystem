using LoyaltySystem.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LoyaltySystem.Shared.API.Settings;
using Serilog;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LoyaltySystem.Shared.API.Services;

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger _logger;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public JwtService(
        IOptions<JwtSettings> jwtSettings,
        ILogger logger)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            ClockSkew = TimeSpan.Zero // Optionally reduce clock skew to make expiration time more accurate
        };
    }

    /// <summary>
    /// Generates a JWT token for the specified user
    /// </summary>
    public string GenerateToken(
        string userId, 
        string firstName,
        string lastName,
        string email, 
        IEnumerable<string> roles,
        IDictionary<string, string> additionalClaims = null)
    {
        try
        {
            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, userId),
                new (JwtRegisteredClaimNames.Sub, userId), // Subject claim
                new (JwtRegisteredClaimNames.Email, email),
                new (ClaimTypes.Email, email), // Include both formats for compatibility
                new (ClaimTypes.Name, $"{firstName} {lastName}"),
            };
            
            claims.AddRange(
                roles.Select(
                    role => new Claim(ClaimTypes.Role, role)));
            
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

            _logger.Information("Generated JWT token for user {UserId} with roles: {Roles}", 
                userId, string.Join(", ", roles));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error generating JWT token");
            throw new InvalidOperationException("Error generating JWT token", ex);
        }
    }
    
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out _);
            return principal;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error validating JWT token");
            return null;
        }
    }
    
    public bool TryParseTokenFromAuthHeader(string authHeader, out string token)
    {
        token = null;
        
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return false;
            
        token = authHeader.Substring("Bearer ".Length).Trim();
        return !string.IsNullOrEmpty(token);
    }
    
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    
    public Guid? GetUserIdFromToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal is null)
            return null;
        
        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
            return userId;
        
        var nameIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(nameIdClaim) && Guid.TryParse(nameIdClaim, out var nameId))
            return nameId;
        
        _logger.Warning("No valid user ID found in token");
        return null;
    }

    public string GetRoleFromToken(string token)
    {
        var principal = ValidateToken(token);
        return principal?.FindFirst(ClaimTypes.Role)?.Value;
    }
    
    public bool IsTokenValid(string token) => 
        ValidateToken(token) is { };
    
    public DateTime GetTokenExpirationTime(string token)
    {
        if (string.IsNullOrEmpty(token))
            return DateTime.MinValue;

        try
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            
            var expClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp);
            if (expClaim is null)
                return DateTime.MinValue;
            
            var unixTime = long.Parse(expClaim.Value);
            var expiryDateTime = DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime;
            
            return expiryDateTime;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Error extracting token expiration time");
            return DateTime.MinValue;
        }
    }
}