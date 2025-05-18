using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Application.Interfaces.Auth;

public interface IAuthenticationService 
{
    /// <summary>
/// Authenticates a user based on the provided login request data.
/// </summary>
/// <param name="dto">The login request containing user credentials.</param>
/// <returns>An operation result containing the login response data.</returns>
Task<OperationResult<LoginResponseDto>> AuthenticateAsync(LoginRequestDto dto);
}
