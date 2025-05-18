using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.DTOs;

public class GetRolesResponseDto 
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolesResponseDto"/> class with the specified user ID and roles.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="roles">The list of roles assigned to the user.</param>
    public GetRolesResponseDto(UserId userId, List<RoleType> roles)
    {
        UserId = userId.ToString();
        Roles = roles;
    }
    
    public string UserId { get; set; }
    public List<RoleType> Roles { get; set; }
}
