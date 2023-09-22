using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.Controllers;

[ApiController]
[Route("api/businesses")]
public class BusinessesController : ControllerBase
{
    private readonly IBusinessService _businessService;
    public BusinessesController(IBusinessService businessService) => _businessService = businessService;
    
    [HttpPost]
    public async Task<IActionResult> CreateBusiness([FromBody] Business newBusiness)
    {
        var createdBusiness = await _businessService.CreateAsync(newBusiness);
        return CreatedAtAction(nameof(GetBusiness), new { id = createdBusiness.Id }, createdBusiness);
    }
    
    [HttpPost]
    [HttpPut]
    [Route("{businessId:guid}/users")]
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBusiness(Guid id) => Ok(await _businessService.GetByIdAsync(id));
}