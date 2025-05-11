using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.PasswordReset;
using LoyaltySystem.Application.DTOs.Auth.Social;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Application.DTOs.Customer;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.Interfaces;

public interface IAuthService 
{
    // Authentication methods
    Task<OperationResult<AuthResponseDto>> AuthenticateAsync(LoginRequestDto dto);
    
    // Specialized registration methods
    Task<OperationResult<UserDto>> RegisterUserAsync(RegisterUserDto registerDto, IEnumerable<RoleType> roles, bool createCustomer = false, CustomerExtraData? customerData = null);
    
    
    // Cross-role functionality
    Task<OperationResult<UserDto>> AddCustomerRoleToUserAsync(string userId);
    
    // Password Reset
    Task<OperationResult> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task<OperationResult> ResetPasswordAsync(ResetPasswordRequestDto request);
    
    
    // Existing profile and user management methods
    Task<OperationResult<UserDto>> UpdateProfileAsync(string userId, UpdateProfileDto updateDto);
    
    // Get User Methods (Email / Id etc)
    Task<OperationResult<UserProfileDto>> GetUserByIdAsync(string userId);
    Task<OperationResult<UserProfileDto>> GetUserByEmailAsync(string email);
    Task<OperationResult<UserProfileDto>> GetUserByUsernameAsync(string username);
    
    /// <summary>
    /// Gets user details by associated customer ID.
    Task<OperationResult<UserDto>> GetUserByCustomerIdAsync(string customerId);
    Task<OperationResult<UserDto>> AddRoleAsync(string userId, RoleType role);
    Task<OperationResult<UserDto>> RemoveRoleAsync(string userId, RoleType role);
    Task<OperationResult<UserDto>> LinkCustomerAsync(string userId, string customerId);
}
