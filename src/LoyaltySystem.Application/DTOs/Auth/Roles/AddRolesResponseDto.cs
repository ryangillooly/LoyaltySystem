using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.DTOs;

public class AddRolesResponseDto 
{
    public AddRolesResponseDto
    (
        UserId userId, 
        List<RoleType> addedRoles, 
        List<RoleType> currentRoles
    )
    {
        Message = "Roles added successfully.";
        UserId = userId.ToString();
        AddedRoles = addedRoles;
        CurrentRoles = currentRoles;
    }
    
    public string Message { get; set; }
    public string UserId { get; set; }
    public List<RoleType> AddedRoles { get; set; }
    public List<RoleType> CurrentRoles { get; set; }
}
