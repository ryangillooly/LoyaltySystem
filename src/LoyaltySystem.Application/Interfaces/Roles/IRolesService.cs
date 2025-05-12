using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.Interfaces.Roles;

public interface IRolesService 
{
    Task<OperationResult<UserDto>> AddCustomerRoleToUserAsync(string userId);
    Task<OperationResult<UserDto>> AddRoleAsync(string userId, List<RoleType> roles);
    Task<OperationResult<UserDto>> RemoveRoleAsync(string userId, List<RoleType> role);
}
