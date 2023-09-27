using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.Controllers;

[ApiController]
[Route("api/businesses/{businessId:guid}/users")]
public class BusinessUsersController : ControllerBase
{
    private readonly IBusinessService _businessService;
    public BusinessUsersController(IBusinessService businessService) => _businessService = businessService;
    
    [HttpPost]
    [HttpPut]
    public async Task<bool> PutUserPermission(Guid businessId, [FromBody] List<Permission> permissions)
    {
        var permissionList = new List<Permission>();
            
        foreach (var permission in permissions)
        {
            permissionList.Add
            (
                new Permission
                {
                    UserId = permission.UserId,
                    BusinessId = businessId,
                    Role = Enum.Parse<UserRole>(permission.Role.ToString())
                }
            );
        }
        await _businessService.UpdatePermissionsAsync(permissionList);
        return true;
    }
}