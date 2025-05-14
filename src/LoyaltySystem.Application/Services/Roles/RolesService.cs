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
        var user = await _userRepository.GetByIdAsync(UserId.FromString(userId));
        if (user is null)
            return OperationResult<UserDto>.FailureResult("User not found.");

        if (user.CustomerId is null)
            return OperationResult<UserDto>.FailureResult("User is not linked to a customer record.");

        await _userRepository.AddRoleAsync(user.Id, new List<RoleType> { RoleType.Customer });
        
        var updatedUser = await _userRepository.GetByIdAsync(user.Id);
        return OperationResult<UserDto>.SuccessResult(UserDto.From(updatedUser!));
    }
    
    public async Task<OperationResult<UserDto>> AddRoleAsync(string userId, List<RoleType> roles)
    {
        var user = await _userRepository.GetByIdAsync(UserId.FromString(userId));
        if (user is null)
            return OperationResult<UserDto>.FailureResult("User not found.");
            
        await _userRepository.AddRoleAsync(user.Id, roles);

        var updatedUser = await _userRepository.GetByIdAsync(user.Id);
        return OperationResult<UserDto>.SuccessResult(UserDto.From(updatedUser!)); 
    }
    
    public async Task<OperationResult<UserDto>> RemoveRoleAsync(string userId, List<RoleType> roles)
    {
        var user = await _userRepository.GetByIdAsync(UserId.FromString(userId));
        if (user is null)
            return OperationResult<UserDto>.FailureResult("User not found.");
            
        await _userRepository.RemoveRoleAsync(user.Id, roles);
        
        var updatedUser = await _userRepository.GetByIdAsync(user.Id);
        return OperationResult<UserDto>.SuccessResult(UserDto.From(updatedUser!)); 
    }
}
