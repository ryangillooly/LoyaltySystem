using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.Social;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.Interfaces;

public interface ISocialAuthService 
{
    /// <summary>
        /// Authenticates a user using social login, validating allowed roles and optionally registering the user if necessary.
        /// </summary>
        /// <param name="request">The social authentication request details.</param>
        /// <param name="allowedRoles">The set of roles permitted to authenticate via this method.</param>
        /// <param name="registerUserAsync">A delegate to asynchronously register a new user if required.</param>
        /// <returns>The result of the social authentication process, including success status and response data.</returns>
        Task<OperationResult<SocialAuthResponseDto>> AuthenticateAsync(
        SocialAuthRequestDto request,
        IEnumerable<RoleType> allowedRoles,
        Func<RegisterUserRequestDto, Task<OperationResult<RegisterUserResponseDto>>> registerUserAsync);
}