using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.Interfaces.Roles;

public interface IRolesService 
{
    Task<OperationResult<InternalUserDto>> AddCustomerRoleToUserAsync(string userId);
    Task<OperationResult<InternalUserDto>> AddRoleAsync(string userId, List<RoleType> roles);
    Task<OperationResult<InternalUserDto>> RemoveRoleAsync(string userId, List<RoleType> role);
}
