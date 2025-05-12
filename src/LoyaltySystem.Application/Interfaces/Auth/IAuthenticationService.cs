using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Application.Interfaces.Auth;

public interface IAuthenticationService 
{
    Task<OperationResult<AuthResponseDto>> AuthenticateAsync(LoginRequestDto dto);
}
