using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.Social;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.Interfaces;

public interface ISocialAuthService 
{
    Task<OperationResult<SocialAuthResponseDto>> AuthenticateAsync(
        SocialAuthRequestDto request,
        IEnumerable<RoleType> allowedRoles,
        Func<RegisterUserDto, Task<OperationResult<InternalUserDto>>> registerUserAsync);
}