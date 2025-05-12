using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Application.Interfaces.Profile;

public interface IProfileService 
{
    Task<OperationResult<UserProfileDto>> GetUserByIdAsync(string userId);
    Task<OperationResult<UserProfileDto>> GetUserByEmailAsync(string email);
    Task<OperationResult<UserProfileDto>> GetUserByUsernameAsync(string username);

    Task<OperationResult<UserDto>> GetUserByCustomerIdAsync(string customerId);
    Task<OperationResult<UserDto>> UpdateProfileAsync(string userId, UpdateProfileDto updateDto);    
}
