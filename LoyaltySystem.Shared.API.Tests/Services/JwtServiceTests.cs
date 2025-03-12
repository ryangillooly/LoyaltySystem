using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Interfaces;
using LoyaltySystem.Shared.API.Services;
using LoyaltySystem.Shared.API.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace LoyaltySystem.Shared.API.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
        private readonly Mock<ILogger<JwtService>> _loggerMock;
        private readonly JwtService _sut; // System Under Test
        private readonly string _jwtSecret = "thisIsAVeryLongSecretKeyUsedForTestingPurposesOnly_MakeItLongForSecurity";
        private readonly string _issuer = "test.issuer";
        private readonly string _audience = "test.audience";
        private readonly int _expirationMinutes = 60;

        public JwtServiceTests()
        {
            // Setup JwtSettings
            var jwtSettings = new JwtSettings
            {
                Secret = _jwtSecret,
                Issuer = _issuer,
                Audience = _audience,
                ExpirationMinutes = _expirationMinutes
            };

            // Setup IOptions for JwtSettings
            _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
            _jwtSettingsMock.Setup(x => x.Value).Returns(jwtSettings);

            // Setup logger
            _loggerMock = new Mock<ILogger<JwtService>>();

            // Create the System Under Test
            _sut = new JwtService(_jwtSettingsMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void GenerateToken_ShouldReturnValidJwtToken()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var username = "testuser";
            var email = "test@example.com";
            var roles = new[] { RoleType.Customer.ToString(), RoleType.Admin.ToString() };
            var customClaims = new Dictionary<string, string>
            {
                { "customerId", Guid.NewGuid().ToString() }
            };

            // Act
            var token = _sut.GenerateToken(userId.ToString(), username, email, roles, customClaims);

            // Assert
            token.Should().NotBeNullOrEmpty();
            
            // Validate the token structure
            var handler = new JwtSecurityTokenHandler();
            var isValidToken = handler.CanReadToken(token);
            isValidToken.Should().BeTrue();
            
            // Parse the token to validate claims
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            jwtToken.Should().NotBeNull();
            
            // Validate issuer, audience and other standard properties
            jwtToken!.Issuer.Should().Be(_issuer);
            jwtToken.Audiences.Should().Contain(_audience);
            
            // Validate claims
            jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value.Should().Be(userId.ToString());
            jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)?.Value.Should().Be(username);
            jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value.Should().Be(email);
            
            // Validate roles
            var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            roleClaims.Should().HaveCount(2);
            roleClaims.Should().Contain(roles);
            
            // Validate custom claims
            jwtToken.Claims.FirstOrDefault(c => c.Type == "customerId")?.Value.Should().Be(customClaims["customerId"]);
            
            // Validate expiration
            var expiration = jwtToken.ValidTo;
            var now = DateTime.UtcNow;
            expiration.Should().BeCloseTo(now.AddMinutes(_expirationMinutes), TimeSpan.FromMinutes(1));
        }

        [Fact]
        public void IsTokenValid_WithValidToken_ShouldReturnTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var token = GenerateValidToken(userId.ToString());

            // Act
            var result = _sut.IsTokenValid(token);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsTokenValid_WithInvalidToken_ShouldReturnFalse()
        {
            // Arrange
            var invalidToken = "invalid.token.structure";

            // Act
            var result = _sut.IsTokenValid(invalidToken);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsTokenValid_WithExpiredToken_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expiredToken = GenerateExpiredToken(userId.ToString());

            // Act
            var result = _sut.IsTokenValid(expiredToken);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateToken_ShouldReturnClaimsPrincipal()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var token = GenerateValidToken(userId.ToString());

            // Act
            var result = _sut.ValidateToken(token);

            // Assert
            result.Should().NotBeNull();
            result.Identity.Should().NotBeNull();
            result.Identity!.IsAuthenticated.Should().BeTrue();
            result.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value.Should().Be(userId.ToString());
        }

        [Fact]
        public void GetUserIdFromToken_WithValidToken_ShouldReturnUserId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var token = GenerateValidToken(userId.ToString());

            // Act
            var result = _sut.GetUserIdFromToken(token);

            // Assert
            result.Should().NotBeNull("because a valid token should return a user ID");
            result.Value.Should().Be(userId, "because the extracted ID should match the original");
        }
        
        [Fact]
        public void Jwt_ClaimMapping_TestBoth()
        {
            // This test verifies how JWT claims are mapped during token validation
            
            // Arrange
            var userId = Guid.NewGuid();
            var token = GenerateValidToken(userId.ToString());
            var handler = new JwtSecurityTokenHandler();
            
            // Read the raw token to check its claims
            var jwt = handler.ReadToken(token) as JwtSecurityToken;
            var originalSubClaim = jwt!.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            originalSubClaim.Should().NotBeNull();
            originalSubClaim!.Value.Should().Be(userId.ToString());
            
            // Validate the token
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret)),
                ClockSkew = TimeSpan.Zero
            };
            
            var principal = handler.ValidateToken(token, validationParams, out _);
            
            // Check what happened to the claims during validation
            var subClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
            var nameIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            
            // Log the claim mapping for debugging
            Console.WriteLine("Original claim from token: " + originalSubClaim.Type + ": " + originalSubClaim.Value);
            Console.WriteLine("Sub claim after validation: " + (subClaim?.Type ?? "null") + ": " + (subClaim?.Value ?? "null"));
            Console.WriteLine("NameId claim after validation: " + (nameIdClaim?.Type ?? "null") + ": " + (nameIdClaim?.Value ?? "null"));
            
            // Assert - the updated JwtService should handle whatever mapping occurs
            var result = _sut.GetUserIdFromToken(token);
            result.Should().NotBeNull();
            result.Value.Should().Be(userId);
        }

        [Fact]
        public void GetRoleFromToken_WithValidToken_ShouldReturnRole()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var token = GenerateValidToken(userId.ToString());

            // Act
            var result = _sut.GetRoleFromToken(token);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(RoleType.Customer.ToString());
        }
        
        [Fact]
        public void GetTokenExpirationTime_WithValidToken_ShouldReturnExpirationTime()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var token = GenerateValidToken(userId.ToString());
            var expectedExpiration = DateTime.UtcNow.AddMinutes(_expirationMinutes);

            // Act
            var result = _sut.GetTokenExpirationTime(token);

            // Assert
            result.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(1));
        }
        
        [Fact]
        public void TryParseTokenFromAuthHeader_WithValidHeader_ShouldReturnTrue()
        {
            // Arrange
            var token = "valid.jwt.token";
            var authHeader = $"Bearer {token}";
            
            // Act
            var result = _sut.TryParseTokenFromAuthHeader(authHeader, out var extractedToken);
            
            // Assert
            result.Should().BeTrue();
            extractedToken.Should().Be(token);
        }
        
        [Fact]
        public void TryParseTokenFromAuthHeader_WithInvalidHeader_ShouldReturnFalse()
        {
            // Arrange
            var authHeader = "Invalid header format";
            
            // Act
            var result = _sut.TryParseTokenFromAuthHeader(authHeader, out var extractedToken);
            
            // Assert
            result.Should().BeFalse();
            extractedToken.Should().BeNull();
        }
        
        [Fact]
        public void GenerateRefreshToken_ShouldReturnUniqueString()
        {
            // Act
            var token1 = _sut.GenerateRefreshToken();
            var token2 = _sut.GenerateRefreshToken();
            
            // Assert
            token1.Should().NotBeNullOrEmpty();
            token2.Should().NotBeNullOrEmpty();
            token1.Should().NotBe(token2); // Should be unique
        }
        
        [Fact]
        public void Debug_ValidateTokenAndExtractUserId()
        {
            // This test directly follows the implementation logic to diagnose issues
            
            // Arrange
            var userId = Guid.NewGuid();
            var token = GenerateValidToken(userId.ToString());
            var handler = new JwtSecurityTokenHandler();
            
            // Decode and examine the token
            var jwt = handler.ReadToken(token) as JwtSecurityToken;
            var subClaim = jwt!.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            Assert.NotNull(subClaim);
            Assert.Equal(userId.ToString(), subClaim.Value);
            
            // First step in GetUserIdFromToken: call ValidateToken
            var principal = _sut.ValidateToken(token);
            Assert.NotNull(principal);
            
            // Second step: find the subject claim (try both formats)
            var userIdFromSubClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var userIdFromNameIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            // Print all claims for debugging
            Console.WriteLine("All claims in the principal:");
            foreach (var claim in principal.Claims)
            {
                Console.WriteLine($"  {claim.Type}: {claim.Value}");
            }
            
            // At least one of the claim formats should contain our user ID
            Assert.True(
                !string.IsNullOrEmpty(userIdFromSubClaim) || !string.IsNullOrEmpty(userIdFromNameIdClaim),
                "Expected to find user ID in either sub or nameidentifier claim"
            );
            
            // If sub claim is present, it should have the correct value
            if (!string.IsNullOrEmpty(userIdFromSubClaim))
            {
                Assert.Equal(userId.ToString(), userIdFromSubClaim);
                
                // Third step: try to parse the userId as a Guid
                var parsedSuccessfully = Guid.TryParse(userIdFromSubClaim, out var parsedUserId);
                Assert.True(parsedSuccessfully);
                Assert.Equal(userId, parsedUserId);
            }
            
            // If nameidentifier claim is present, it should have the correct value
            if (!string.IsNullOrEmpty(userIdFromNameIdClaim))
            {
                Assert.Equal(userId.ToString(), userIdFromNameIdClaim);
                
                // Third step: try to parse the userId as a Guid
                var parsedSuccessfully = Guid.TryParse(userIdFromNameIdClaim, out var parsedUserId);
                Assert.True(parsedSuccessfully);
                Assert.Equal(userId, parsedUserId);
            }
            
            // Final step: call the actual method
            var result = _sut.GetUserIdFromToken(token);
            Assert.NotNull(result);
            Assert.Equal(userId, result.Value);
        }
        
        #region Helper Methods
        
        private string GenerateValidToken(string userId)
        {
            // Ensure the userId is a valid Guid
            if (!Guid.TryParse(userId, out var parsedGuid))
            {
                throw new ArgumentException("userId must be a valid Guid", nameof(userId));
            }
            
            // Format without any braces or extra formatting, just the 32 hex digits
            string guidString = parsedGuid.ToString("D");
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret); // Use UTF8 encoding to match the service
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, guidString),
                    new Claim(JwtRegisteredClaimNames.UniqueName, "testuser"),
                    new Claim(JwtRegisteredClaimNames.Email, "test@example.com"),
                    new Claim(ClaimTypes.Role, RoleType.Customer.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature
                )
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        
        private string GenerateExpiredToken(string userId)
        {
            // Ensure the userId is a valid Guid
            if (!Guid.TryParse(userId, out var parsedGuid))
            {
                throw new ArgumentException("userId must be a valid Guid", nameof(userId));
            }
            
            // Format without any braces or extra formatting, just the 32 hex digits
            string guidString = parsedGuid.ToString("D");
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret); // Use UTF8 encoding to match the service
            
            // Using a fixed point in the past to ensure NotBefore is always before Expires
            var pastTime = DateTime.UtcNow.AddHours(-2); // 2 hours ago
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, guidString),
                    new Claim(JwtRegisteredClaimNames.UniqueName, "testuser"),
                    new Claim(JwtRegisteredClaimNames.Email, "test@example.com"),
                    new Claim(ClaimTypes.Role, RoleType.Customer.ToString())
                }),
                NotBefore = pastTime, // Token valid starting from 2 hours ago
                Expires = pastTime.AddHours(1), // Token expired 1 hour ago
                IssuedAt = pastTime, // Token was issued 2 hours ago
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature
                )
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        
        private string GenerateTokenWithRoles(string userId, string[] roles)
        {
            // Ensure the userId is a valid Guid
            if (!Guid.TryParse(userId, out var parsedGuid))
            {
                throw new ArgumentException("userId must be a valid Guid", nameof(userId));
            }
            
            // Format without any braces or extra formatting, just the 32 hex digits
            string guidString = parsedGuid.ToString("D");
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret); // Use UTF8 encoding to match the service
            
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, guidString),
                new Claim(JwtRegisteredClaimNames.UniqueName, "testuser"),
                new Claim(JwtRegisteredClaimNames.Email, "test@example.com")
            };
            
            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature
                )
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        
        #endregion
    }
}