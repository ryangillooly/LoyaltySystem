using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.Interfaces;

public interface IAuthService 
{
    Task<OperationResult<AuthResponseDto>> AuthenticateAsync(string username, string password);
    Task<OperationResult<UserDto>> RegisterAsync(RegisterUserDto registerDto);
    Task<OperationResult<UserDto>> UpdateProfileAsync(string userId, UpdateProfileDto updateDto);
    Task<OperationResult<UserDto>> GetUserByIdAsync(string userId);
    Task<OperationResult<UserDto>> GetUserByCustomerIdAsync(string customerId);
    Task<OperationResult<UserDto>> AddRoleAsync(string userId, RoleType role);
    Task<OperationResult<UserDto>> RemoveRoleAsync(string userId, RoleType role);
    Task<OperationResult<UserDto>> LinkCustomerAsync(string userId, string customerId);
}
