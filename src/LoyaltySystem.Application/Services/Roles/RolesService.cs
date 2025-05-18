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

    /// <summary>
    /// Initializes a new instance of the <see cref="RolesService"/> class with the specified user repository and logger.
    /// </summary>
    /// <param name="userRepository">Repository for accessing user data.</param>
    /// <param name="logger">Logger for recording service operations.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="userRepository"/> or <paramref name="logger"/> is null.</exception>
    public RolesService(IUserRepository userRepository, ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }
    
    /// <summary>
    /// Retrieves the roles assigned to a user by their user ID.
    /// </summary>
    /// <param name="userIdString">The string representation of the user's ID.</param>
    /// <returns>An <see cref="OperationResult{GetRolesResponseDto}"/> containing the user's roles if found; otherwise, a failure result.</returns>
    public async Task<OperationResult<GetRolesResponseDto>> GetRolesAsync(string userIdString)
    {
        var userId = UserId.FromString(userIdString);
        
        var roles = await _userRepository.GetRolesAsync(userId);
        if (roles is null)
            return OperationResult<GetRolesResponseDto>.FailureResult("Roles not found");
        
        var response = new GetRolesResponseDto(userId, roles.ToList());
        return OperationResult<GetRolesResponseDto>.SuccessResult(response);
    }

    /// <summary>
    /// Adds the "Customer" role to a user if the user exists and is linked to a customer record, then returns the updated roles.
    /// </summary>
    /// <param name="userIdString">The string representation of the user's ID.</param>
    /// <returns>An operation result containing the user ID, the added role, and the updated list of roles, or a failure result if the user is not found or not linked to a customer.</returns>
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
    
    /// <summary>
    /// Adds specified roles to a user and returns the updated list of roles.
    /// </summary>
    /// <param name="userIdString">The string representation of the user's ID.</param>
    /// <param name="request">The request containing the roles to add.</param>
    /// <returns>An operation result containing the user ID, added roles, and the updated roles list.</returns>
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
    
    /// <summary>
    /// Removes specified roles from a user and returns the updated list of roles.
    /// </summary>
    /// <param name="userIdString">The string representation of the user's ID.</param>
    /// <param name="request">The request containing the roles to remove.</param>
    /// <returns>An operation result containing the removed roles and the user's updated roles list, or a failure result if the user is not found.</returns>
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
