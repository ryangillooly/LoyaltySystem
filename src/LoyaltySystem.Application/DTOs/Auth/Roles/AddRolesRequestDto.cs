using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.DTOs;

public class AddRolesRequestDto 
{
    public List<RoleType> Roles { get; set; }
}
