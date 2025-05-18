using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.Interfaces.Roles;

public interface IRolesService 
{
    /// <summary>
/// Retrieves the roles assigned to the user identified by the specified user ID.
/// </summary>
/// <param name="userIdString">The unique identifier of the user whose roles are to be retrieved.</param>
/// <returns>An operation result containing the user's roles information.</returns>
Task<OperationResult<GetRolesResponseDto>> GetRolesAsync(string userIdString);
    /// <summary>
/// Assigns the customer role to the specified user.
/// </summary>
/// <param name="userIdString">The identifier of the user to whom the customer role will be added.</param>
/// <returns>An operation result containing the outcome of the role assignment.</returns>
Task<OperationResult<AddRolesResponseDto>> AddCustomerRoleToUserAsync(string userIdString);
    /// <summary>
/// Asynchronously adds specified roles to a user based on the provided request details.
/// </summary>
/// <param name="userIdString">The identifier of the user to whom roles will be added.</param>
/// <param name="request">The details of the roles to add.</param>
/// <returns>An operation result containing the outcome and details of the role addition.</returns>
Task<OperationResult<AddRolesResponseDto>> AddRoleAsync(string userIdString, AddRolesRequestDto request);
    /// <summary>
/// Removes specified roles from a user.
/// </summary>
/// <param name="userIdString">The identifier of the user whose roles are to be removed.</param>
/// <param name="request">Details of the roles to remove.</param>
/// <returns>An operation result containing the outcome of the role removal.</returns>
Task<OperationResult<RemoveRolesResponseDto>> RemoveRoleAsync(string userIdString, RemoveRolesRequestDto request);
}
