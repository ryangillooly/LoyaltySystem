using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.DTOs;

public class RemoveRolesResponseDto 
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveRolesResponseDto"/> class with information about a user's removed and current roles after a role removal operation.
    /// </summary>
    /// <param name="userId">The identifier of the user whose roles were modified.</param>
    /// <param name="removedRoles">The list of roles that have been removed from the user.</param>
    /// <param name="currentRoles">The list of roles currently assigned to the user after removal.</param>
    public RemoveRolesResponseDto
    (
        UserId userId, 
        List<RoleType> removedRoles, 
        List<RoleType> currentRoles
    )
    {
        Message = "Roles removed successfully.";
        UserId = userId.ToString();
        RemovedRoles = removedRoles;
        CurrentRoles = currentRoles;
    }
    
    public string Message { get; set; }
    public string UserId { get; set; }
    public List<RoleType> RemovedRoles { get; set; }
    public List<RoleType> CurrentRoles { get; set; }    
}
