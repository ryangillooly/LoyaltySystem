using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.DTOs;

public class AddRolesResponseDto 
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddRolesResponseDto"/> class with the specified user ID, added roles, and current roles.
    /// </summary>
    /// <param name="userId">The identifier of the user to whom roles were added.</param>
    /// <param name="addedRoles">The list of roles that were added to the user.</param>
    /// <param name="currentRoles">The list of the user's roles after the addition.</param>
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
