using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces.Roles;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Repositories;
using Serilog;

namespace LoyaltySystem.Application.Services.Roles;

public class RolesService : IRolesService 
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger _logger;

    public RolesService(IUserRepository userRepository, ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }
    
    public async Task<OperationResult<UserDto>> AddCustomerRoleToUserAsync(string userId)
    {
        UserId userIdObj;
        try
        {
            userIdObj = UserId.FromString(userId);
        }
        catch (FormatException ex)
        {
            return OperationResult<UserDto>.FailureResult($"Invalid UserId format: {ex.Message}");
        }

        var user = await _userRepository.GetByIdAsync(userIdObj);
        if (user is null)
            return OperationResult<UserDto>.FailureResult("User not found.");

        if (user.CustomerId is null)
            return OperationResult<UserDto>.FailureResult("User is not linked to a customer record.");

        await _userRepository.AddRoleAsync(userIdObj, new List<RoleType> { RoleType.Customer });
        
        var updatedUser = await _userRepository.GetByIdAsync(userIdObj);
        return OperationResult<UserDto>.SuccessResult(UserDto.From(updatedUser!));
    }
    
    public async Task<OperationResult<UserDto>> AddRoleAsync(string userId, List<RoleType> roles)
    {
        UserId userIdObj;
        try
        {
            userIdObj = UserId.FromString(userId);
        }
        catch (FormatException ex)
        {
            return OperationResult<UserDto>.FailureResult($"Invalid UserId format: {ex.Message}");
        }

        var user = await _userRepository.GetByIdAsync(userIdObj);
        if (user is null)
            return OperationResult<UserDto>.FailureResult("User not found.");
            
        await _userRepository.AddRoleAsync(userIdObj, roles);
        // Re-fetch user to get updated roles for the DTO
        var updatedUser = await _userRepository.GetByIdAsync(userIdObj);
        
        // Temporarily mapping to UserDto
        Func<User, UserDto> tempMapper = u => new UserDto 
        {
            Id = u.Id.Value, 
            PrefixedId = u.PrefixedId ?? string.Empty, // Fallback
            FirstName = u.FirstName ?? string.Empty, // Fallback
            LastName = u.LastName ?? string.Empty, // Fallback
            UserName = u.UserName ?? string.Empty, // Fallback
            Email = u.Email ?? string.Empty, // Fallback
            Status = u.Status.ToString(), 
            CustomerId = u.CustomerId?.ToString(), // Already handles null
            CreatedAt = u.CreatedAt, 
            LastLoginAt = u.LastLoginAt, 
            Roles = u.Roles.Select(r => r.Role.ToString()).ToList(),
            IsEmailConfirmed = u.IsEmailConfirmed
        };
        
        return OperationResult<UserDto>.SuccessResult(tempMapper(updatedUser!)); 
    }
    
    public async Task<OperationResult<UserDto>> RemoveRoleAsync(string userId, List<RoleType> roles)
    {
        UserId userIdObj;
        try
        {
            userIdObj = UserId.FromString(userId);
        }
        catch (FormatException ex)
        {
            return OperationResult<UserDto>.FailureResult($"Invalid UserId format: {ex.Message}");
        }

        var user = await _userRepository.GetByIdAsync(userIdObj);
        if (user is null)
            return OperationResult<UserDto>.FailureResult("User not found.");
            
        await _userRepository.RemoveRoleAsync(userIdObj, roles);
        
        // Re-fetch user to get updated roles for the DTO
        var updatedUser = await _userRepository.GetByIdAsync(userIdObj);
        
        // Temporarily mapping to UserDto
        Func<User, UserDto> tempMapper = u => new UserDto 
        {
            Id = u.Id.Value, 
            PrefixedId = u.PrefixedId, 
            FirstName = u.FirstName, 
            LastName = u.LastName,
            UserName = u.UserName, 
            Email = u.Email, 
            Status = u.Status.ToString(), 
            CustomerId = u.CustomerId?.ToString(),
            CreatedAt = u.CreatedAt, 
            LastLoginAt = u.LastLoginAt, 
            Roles = u.Roles.Select(r => r.Role.ToString()).ToList(),
            IsEmailConfirmed = u.IsEmailConfirmed
        };

        return OperationResult<UserDto>.SuccessResult(tempMapper(updatedUser!)); 
    }
}
