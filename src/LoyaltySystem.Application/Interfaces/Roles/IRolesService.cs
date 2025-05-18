using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.Interfaces.Roles;

public interface IRolesService 
{
    Task<OperationResult<GetRolesResponseDto>> GetRolesAsync(string userIdString);
    Task<OperationResult<AddRolesResponseDto>> AddCustomerRoleToUserAsync(string userIdString);
    Task<OperationResult<AddRolesResponseDto>> AddRoleAsync(string userIdString, AddRolesRequestDto request);
    Task<OperationResult<RemoveRolesResponseDto>> RemoveRoleAsync(string userIdString, RemoveRolesRequestDto request);
}
