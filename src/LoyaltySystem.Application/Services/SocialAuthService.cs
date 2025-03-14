/*
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Interfaces;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Shared.API.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Linq;
using System.Net.Http;

namespace LoyaltySystem.Application.Services;

public class SocialAuthService : ISocialAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;
    private readonly SocialAuthSettings _settings;
    private readonly ILogger<SocialAuthService> _logger;
    private readonly IJwtService _jwtService;

    public SocialAuthService(
        IUserRepository userRepository,
        IAuthService authService,
        IOptions<SocialAuthSettings> settings,
        IJwtService jwtService,
        ILogger<SocialAuthService> logger)
    {
        _userRepository = userRepository;
        _authService = authService;
        _settings = settings.Value;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<OperationResult<AuthResponseDto>> AuthenticateWithGoogleAsync(string authCode)
    {
        try
        {
            var settings = _settings.Google;
            var payload = await GoogleJsonWebSignature.ValidateAsync(authCode, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { settings.ClientId }
            });

            // Check if user exists
            var user = await _userRepository.GetByEmailAsync(payload.Email);
            if (user == null)
            {
                // Create new user
                var registerDto = new RegisterUserDto
                {
                    Email = payload.Email,
                    Username = payload.Email,
                    FirstName = payload.GivenName,
                    LastName = payload.FamilyName,
                    Password = GenerateSecurePassword() // Generate a secure random password
                };

                var registerResult = await _authService.RegisterAsync(registerDto);
                if (!registerResult.Success)
                    return OperationResult<AuthResponseDto>.FailureResult(registerResult.Errors);

                user = await _userRepository.GetByIdAsync(registerResult.Data.Id);
                await _authService.AddRoleAsync(user.Id.ToString(), RoleType.Customer);
            }

            // Update social login info
            user.UpdateSocialLogin(SocialLoginProvider.Google, payload.Subject, payload.Email);
            await _userRepository.UpdateAsync(user);

            // Generate token
            var token = _jwtService.GenerateToken(
                user.Id.ToString(),
                user.Username,
                user.Email,
                user.Roles.Select(r => r.Role.ToString()),
                new Dictionary<string, string>
                {
                    { "SocialProvider", "Google" },
                    { "SocialId", payload.Subject }
                });

            return OperationResult<AuthResponseDto>.SuccessResult(new AuthResponseDto
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id.ToString(),
                    Email = user.Email,
                    Username = user.Username
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating with Google");
            return OperationResult<AuthResponseDto>.FailureResult("Error authenticating with Google");
        }
    }

    public async Task<OperationResult<AuthResponseDto>> AuthenticateWithAppleAsync(string authCode, string nonce)
    {
        try
        {
            var settings = _settings.Apple;
            
            // Validate the identity token from Apple
            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "https://appleid.apple.com",
                ValidateAudience = true,
                ValidAudience = settings.ClientId,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = await GetApplePublicKey(),
                ValidateNonce = true // Apple specific
            };

            var tokenValidationResult = await handler.ValidateTokenAsync(authCode, validationParameters);
            if (!tokenValidationResult.IsValid)
            {
                _logger.LogWarning("Invalid Apple token");
                return OperationResult<AuthResponseDto>.FailureResult("Invalid Apple token");
            }

            var payload = tokenValidationResult.Claims;
            var email = payload.GetValueOrDefault("email") as string;
            var sub = payload.GetValueOrDefault("sub") as string;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(sub))
            {
                _logger.LogWarning("Missing required claims from Apple token");
                return OperationResult<AuthResponseDto>.FailureResult("Invalid token claims");
            }

            // Validate nonce to prevent replay attacks
            var tokenNonce = payload.GetValueOrDefault("nonce") as string;
            if (tokenNonce != nonce)
            {
                _logger.LogWarning("Invalid nonce in Apple token");
                return OperationResult<AuthResponseDto>.FailureResult("Invalid token nonce");
            }

            // Check if user exists
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                // Create new user
                var registerDto = new RegisterUserDto
                {
                    Email = email,
                    Username = email, // Apple doesn't provide username
                    FirstName = payload.GetValueOrDefault("given_name") as string ?? "",
                    LastName = payload.GetValueOrDefault("family_name") as string ?? "",
                    Password = GenerateSecurePassword()
                };

                var registerResult = await _authService.RegisterAsync(registerDto);
                if (!registerResult.Success)
                    return OperationResult<AuthResponseDto>.FailureResult(registerResult.Errors);

                user = await _userRepository.GetByIdAsync(registerResult.Data.Id);
                await _authService.AddRoleAsync(user.Id.ToString(), RoleType.Customer);
            }

            // Update social login info
            user.UpdateSocialLogin(SocialLoginProvider.Apple, sub, email);
            await _userRepository.UpdateAsync(user);

            // Generate token
            var token = _jwtService.GenerateToken(
                user.Id.ToString(),
                user.Username,
                user.Email,
                user.Roles.Select(r => r.Role.ToString()),
                new Dictionary<string, string>
                {
                    { "SocialProvider", "Apple" },
                    { "SocialId", sub }
                });

            return OperationResult<AuthResponseDto>.SuccessResult(new AuthResponseDto
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id.ToString(),
                    Email = user.Email,
                    Username = user.Username
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating with Apple");
            return OperationResult<AuthResponseDto>.FailureResult("Error authenticating with Apple");
        }
    }

    public Task<OperationResult<string>> GetGoogleAuthUrlAsync(string state)
    {
        var settings = _settings.Google;
        var url = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                 $"client_id={settings.ClientId}&" +
                 $"response_type=code&" +
                 $"scope={string.Join(" ", settings.Scopes)}&" +
                 $"redirect_uri={Uri.EscapeDataString(settings.RedirectUri)}&" +
                 $"state={state}";

        return Task.FromResult(OperationResult<string>.SuccessResult(url));
    }

    public Task<OperationResult<string>> GetAppleAuthUrlAsync(string state, string nonce)
    {
        var settings = _settings.Apple;
        var url = $"https://appleid.apple.com/auth/authorize?" +
                 $"client_id={settings.ClientId}&" +
                 $"response_type=code&" +
                 $"scope={string.Join(" ", settings.Scopes)}&" +
                 $"redirect_uri={Uri.EscapeDataString(settings.RedirectUri)}&" +
                 $"state={state}&" +
                 $"nonce={nonce}";

        return Task.FromResult(OperationResult<string>.SuccessResult(url));
    }

    public async Task<OperationResult<UserDto>> LinkGoogleAccountAsync(string userId, string authCode)
    {
        try
        {
            var settings = _settings.Google;
            var payload = await GoogleJsonWebSignature.ValidateAsync(authCode, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { settings.ClientId }
            });

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return OperationResult<UserDto>.FailureResult("User not found");

            // Check if Google ID is already linked to another account
            var existingUser = await _userRepository.GetByGoogleIdAsync(payload.Subject);
            if (existingUser != null && existingUser.Id.ToString() != userId)
                return OperationResult<UserDto>.FailureResult("Google account already linked to another user");

            user.UpdateSocialLogin(SocialLoginProvider.Google, payload.Subject, payload.Email);
            await _userRepository.UpdateAsync(user);

            return OperationResult<UserDto>.SuccessResult(new UserDto
            {
                Id = user.Id.ToString(),
                Email = user.Email,
                Username = user.Username
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking Google account");
            return OperationResult<UserDto>.FailureResult("Error linking Google account");
        }
    }

    public async Task<OperationResult<UserDto>> LinkAppleAccountAsync(string userId, string authCode, string nonce)
    {
        try
        {
            var settings = _settings.Apple;
            
            // Validate the identity token
            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "https://appleid.apple.com",
                ValidateAudience = true,
                ValidAudience = settings.ClientId,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = await GetApplePublicKey(),
                ValidateNonce = true
            };

            var tokenValidationResult = await handler.ValidateTokenAsync(authCode, validationParameters);
            if (!tokenValidationResult.IsValid)
            {
                return OperationResult<UserDto>.FailureResult("Invalid Apple token");
            }

            var payload = tokenValidationResult.Claims;
            var email = payload.GetValueOrDefault("email") as string;
            var sub = payload.GetValueOrDefault("sub") as string;

            // Validate nonce
            var tokenNonce = payload.GetValueOrDefault("nonce") as string;
            if (tokenNonce != nonce)
            {
                return OperationResult<UserDto>.FailureResult("Invalid token nonce");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return OperationResult<UserDto>.FailureResult("User not found");

            // Check if Apple ID is already linked to another account
            var existingUser = await _userRepository.GetByAppleIdAsync(sub);
            if (existingUser != null && existingUser.Id.ToString() != userId)
                return OperationResult<UserDto>.FailureResult("Apple account already linked to another user");

            // Verify email matches
            if (user.Email != email)
                return OperationResult<UserDto>.FailureResult("Apple account email does not match user email");

            user.UpdateSocialLogin(SocialLoginProvider.Apple, sub, email);
            await _userRepository.UpdateAsync(user);

            return OperationResult<UserDto>.SuccessResult(new UserDto
            {
                Id = user.Id.ToString(),
                Email = user.Email,
                Username = user.Username
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking Apple account");
            return OperationResult<UserDto>.FailureResult("Error linking Apple account");
        }
    }

    public async Task<OperationResult<UserDto>> UnlinkSocialAccountAsync(string userId, SocialLoginProvider provider)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return OperationResult<UserDto>.FailureResult("User not found");

            user.RemoveSocialLogin(provider);
            await _userRepository.UpdateAsync(user);

            return OperationResult<UserDto>.SuccessResult(new UserDto
            {
                Id = user.Id.ToString(),
                Email = user.Email,
                Username = user.Username
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking social account");
            return OperationResult<UserDto>.FailureResult("Error unlinking social account");
        }
    }

    public async Task<OperationResult<bool>> ValidateGoogleTokenAsync(string token)
    {
        try
        {
            var settings = _settings.Google;
            var payload = await GoogleJsonWebSignature.ValidateAsync(token, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { settings.ClientId }
            });

            return OperationResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google token");
            return OperationResult<bool>.FailureResult("Invalid token");
        }
    }

    public async Task<OperationResult<bool>> ValidateAppleTokenAsync(string token)
    {
        try
        {
            var settings = _settings.Apple;
            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "https://appleid.apple.com",
                ValidateAudience = true,
                ValidAudience = settings.ClientId,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = await GetApplePublicKey()
            };

            var tokenValidationResult = await handler.ValidateTokenAsync(token, validationParameters);
            return OperationResult<bool>.SuccessResult(tokenValidationResult.IsValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Apple token");
            return OperationResult<bool>.FailureResult("Invalid token");
        }
    }

    private string GenerateSecurePassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
        var random = new RNGCryptoServiceProvider();
        var bytes = new byte[32];
        random.GetBytes(bytes);

        var result = new StringBuilder();
        foreach (byte b in bytes)
        {
            result.Append(chars[b % chars.Length]);
        }

        return result.ToString();
    }

    private async Task<SecurityKey> GetApplePublicKey()
    {
        // In a production environment, you should cache this key and refresh periodically
        using var client = new HttpClient();
        var response = await client.GetFromJsonAsync<AppleKeyResponse>("https://appleid.apple.com/auth/keys");
        
        // Get the first key (you might want to match the key ID with the one in your token)
        var key = response.Keys.First();
        
        return new RsaSecurityKey(new RSAParameters
        {
            Modulus = Base64UrlEncoder.DecodeBytes(key.N),
            Exponent = Base64UrlEncoder.DecodeBytes(key.E)
        });
    }
}

public class AppleKeyResponse
{
    public List<AppleKey> Keys { get; set; }
}

public class AppleKey
{
    public string Kty { get; set; }
    public string Kid { get; set; }
    public string Use { get; set; }
    public string Alg { get; set; }
    public string N { get; set; }
    public string E { get; set; }
} 
*/