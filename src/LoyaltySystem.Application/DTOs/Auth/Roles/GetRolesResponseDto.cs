using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.DTOs;

public class GetRolesResponseDto 
{
    public GetRolesResponseDto(UserId userId, List<RoleType> roles)
    {
        UserId = userId.ToString();
        Roles = roles;
    }
    
    public string UserId { get; set; }
    public List<RoleType> Roles { get; set; }
}
