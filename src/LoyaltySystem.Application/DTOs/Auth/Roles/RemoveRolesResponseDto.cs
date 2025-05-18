using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.DTOs;

public class RemoveRolesResponseDto 
{
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
