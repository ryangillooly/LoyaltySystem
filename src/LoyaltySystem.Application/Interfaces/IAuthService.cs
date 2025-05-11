using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.Social;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Application.DTOs.Customer;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.Interfaces;

public interface IAuthService 
{
    // Authentication methods
    Task<OperationResult<AuthResponseDto>> AuthenticateAsync(string identifier, string password, LoginIdentifierType identifierType);
    Task<OperationResult<AuthResponseDto>> AuthenticateForAppAsync(string identifier, string password, LoginIdentifierType identifierType, IEnumerable<RoleType> allowedRoles);
    
    
    // Specialized registration methods
    Task<OperationResult<UserDto>> RegisterUserAsync(RegisterUserDto registerDto, IEnumerable<RoleType> roles, bool createCustomer = false, CustomerExtraData? customerData = null);
    
    // Cross-role functionality
    Task<OperationResult<UserDto>> AddCustomerRoleToUserAsync(string userId);
    
    // Existing profile and user management methods
    Task<OperationResult<UserDto>> UpdateProfileAsync(string userId, UpdateProfileDto updateDto);
    
    /// <summary>
    /// Gets user profile details by user ID, potentially including associated customer data.
    /// </summary>
    Task<OperationResult<UserProfileDto>> GetUserByIdAsync(string userId);
    
    /// <summary>
    /// Gets user details by associated customer ID.
    Task<OperationResult<UserDto>> GetUserByCustomerIdAsync(string customerId);
    Task<OperationResult<UserDto>> AddRoleAsync(string userId, RoleType role);
    Task<OperationResult<UserDto>> RemoveRoleAsync(string userId, RoleType role);
    Task<OperationResult<UserDto>> LinkCustomerAsync(string userId, string customerId);
}
