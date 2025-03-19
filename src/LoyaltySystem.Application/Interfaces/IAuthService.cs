using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.Interfaces;

public interface IAuthService 
{
    // Authentication methods
    Task<OperationResult<AuthResponseDto>> AuthenticateAsync(string identifier, string password, LoginIdentifierType identifierType);
    Task<OperationResult<AuthResponseDto>> AuthenticateForAppAsync(string identifier, string password, LoginIdentifierType identifierType, IEnumerable<RoleType> allowedRoles);
    
    // Specialized registration methods
    Task<OperationResult<UserDto>> RegisterCustomerAsync(RegisterUserDto registerDto);
    Task<OperationResult<UserDto>> RegisterStaffAsync(RegisterUserDto registerDto);
    Task<OperationResult<UserDto>> RegisterAdminAsync(RegisterUserDto registerDto);
    
    // Cross-role functionality
    Task<OperationResult<UserDto>> AddCustomerRoleToUserAsync(string userId);
    
    // Existing profile and user management methods
    Task<OperationResult<UserDto>> UpdateProfileAsync(string userId, UpdateProfileDto updateDto);
    Task<OperationResult<UserDto>> GetUserByIdAsync(string userId);
    Task<OperationResult<UserDto>> GetUserByCustomerIdAsync(string customerId);
    Task<OperationResult<UserDto>> AddRoleAsync(string userId, RoleType role);
    Task<OperationResult<UserDto>> RemoveRoleAsync(string userId, RoleType role);
    Task<OperationResult<UserDto>> LinkCustomerAsync(string userId, string customerId);
}
