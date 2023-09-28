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
        return CreatedAtAction(nameof(GetBusiness), new { businessId = createdBusiness.Id }, createdBusiness);
    }
    
    [HttpGet("{businessId}")]
    public async Task<IActionResult> GetBusiness(Guid businessId) => Ok(await _businessService.GetByIdAsync(businessId));
    
    [HttpPost]
    [Route("{businessId:guid}/campaigns")]
    public async Task<IActionResult> CreateCampaign(Guid businessId, [FromBody] Campaign newCampaign)
    {
        newCampaign.BusinessId = businessId;
        var createdCampaign = await _businessService.CreateCampaignAsync(newCampaign);
        return CreatedAtAction(nameof(GetCampaignById), new { businessId = createdCampaign.BusinessId, campaignId = createdCampaign.Id }, createdCampaign);
    }
    
    [HttpGet]
    [Route("{businessId:guid}/campaigns")]
    public async Task<IActionResult> GetCampaigns(Guid businessId) => Ok(await _businessService.GetByIdAsync(businessId));
    
    [HttpGet]
    [Route("{businessId:guid}/campaigns/{campaignId:guid}")]
    public async Task<IActionResult> GetCampaignById(Guid businessId, Guid campaignId) => Ok(await _businessService.GetByIdAsync(businessId));
    
    [HttpPost]
    [HttpPut]
    [Route("{businessId:guid}/users/{userId:guid}")]
    public async Task<bool> PutUserPermission(Guid businessId, Guid userId, [FromBody] List<Permission> permissions)
    {
        var permissionList = new List<Permission>();
            
        foreach (var permission in permissions)
        {
            permissionList.Add
            (
                new Permission
                {
                    UserId = userId,
                    BusinessId = businessId,
                    Role = Enum.Parse<UserRole>(permission.Role.ToString())
                }
            );
        }
        await _businessService.UpdatePermissionsAsync(permissionList);
        return true;
    }
}