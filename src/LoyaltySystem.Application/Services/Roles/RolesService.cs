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
    
    public async Task<OperationResult<GetRolesResponseDto>> GetRolesAsync(string userIdString)
    {
        var userId = UserId.FromString(userIdString);
        
        var roles = await _userRepository.GetRolesAsync(userId);
        if (roles is null)
            return OperationResult<GetRolesResponseDto>.FailureResult("Roles not found");
        
        var response = new GetRolesResponseDto(userId, roles.ToList());
        return OperationResult<GetRolesResponseDto>.SuccessResult(response);
    }

    public async Task<OperationResult<AddRolesResponseDto>> AddCustomerRoleToUserAsync(string userIdString)
    {
        var userId = UserId.FromString(userIdString);
        
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return OperationResult<AddRolesResponseDto>.FailureResult("User not found.");

        if (user.CustomerId is null)
            return OperationResult<AddRolesResponseDto>.FailureResult("User is not linked to a customer record.");

        var role = new List<RoleType> { RoleType.Customer };
        
        await _userRepository.AddRoleAsync(userId, role);
        var updatedRoles = await _userRepository.GetRolesAsync(userId);
        
        var response = new AddRolesResponseDto(user.Id, role, updatedRoles.ToList());
        return OperationResult<AddRolesResponseDto>.SuccessResult(response);
    }
    
    public async Task<OperationResult<AddRolesResponseDto>> AddRoleAsync(string userIdString, AddRolesRequestDto request)
    {
        var userId = UserId.FromString(userIdString);
        
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return OperationResult<AddRolesResponseDto>.FailureResult("User not found.");
            
        await _userRepository.AddRoleAsync(userId, request.Roles);
        var updatedRoles = await _userRepository.GetRolesAsync(userId);
        
        var response = new AddRolesResponseDto(userId, request.Roles, updatedRoles.ToList());
        return OperationResult<AddRolesResponseDto>.SuccessResult(response); 
    }
    
    public async Task<OperationResult<RemoveRolesResponseDto>> RemoveRoleAsync(string userIdString, RemoveRolesRequestDto request)
    {
        var userId = UserId.FromString(userIdString);
        
        var user = await _userRepository.GetRolesAsync(userId);
        if (user is null)
            return OperationResult<RemoveRolesResponseDto>.FailureResult("User not found.");
            
        await _userRepository.RemoveRoleAsync(userId, request.Roles);
        var updatedRoles = await _userRepository.GetRolesAsync(userId);
        
        var response = new RemoveRolesResponseDto(userId, request.Roles, updatedRoles.ToList());
        return OperationResult<RemoveRolesResponseDto>.SuccessResult(response); 
    }
}
