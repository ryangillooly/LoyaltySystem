using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.Interfaces;

public interface ISocialAuthService 
{
    Task<OperationResult<AuthResponseDto>> AuthenticateWithGoogleAsync(string authCode);
    Task<OperationResult<AuthResponseDto>> AuthenticateWithAppleAsync(string authCode, string nonce);
    Task<OperationResult<string>> GetGoogleAuthUrlAsync(string state);
    Task<OperationResult<string>> GetAppleAuthUrlAsync(string state, string nonce);
    Task<OperationResult<UserDto>> LinkGoogleAccountAsync(string userId, string authCode);
    Task<OperationResult<UserDto>> LinkAppleAccountAsync(string userId, string authCode, string nonce);
    Task<OperationResult<UserDto>> UnlinkSocialAccountAsync(string userId, SocialLoginProvider provider);
    Task<OperationResult<bool>> ValidateGoogleTokenAsync(string token);
    Task<OperationResult<bool>> ValidateAppleTokenAsync(string token);
} 