using Amazon.DynamoDBv2.Model;
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
        var createdBusiness = await _businessService.CreateBusinessAsync(newBusiness);
        return CreatedAtAction(nameof(GetBusiness), new { businessId = createdBusiness.Id }, createdBusiness);
    }

    [HttpPut("{businessId:guid}")]
    public async Task<IActionResult> UpdateBusiness(Guid businessId, [FromBody] Business business)
    {
        business.Id = businessId;
        var updatedBusiness = await _businessService.UpdateBusinessAsync(business);
        if (updatedBusiness == null) return NotFound();

        return Ok(updatedBusiness);
    }

    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetBusiness(Guid businessId)
    {
        try
        {
            var card = await _businessService.GetBusinessAsync(businessId);
            return Ok(card);
        }
        catch(ResourceNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch(Exception ex)
        {
            // Handle other exceptions as needed
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{businessId:guid}")]
    public async Task<IActionResult> DeleteBusiness(Guid businessId)
    {
        await _businessService.DeleteBusinessAsync(businessId);
        // Need to make sure that we delete all data related to a Business which is being deleted (i.e. Permissions, Loyalty Cards etc)
        return NoContent();
    }

    [HttpPost("{businessId:guid}/campaigns")]
    public async Task<IActionResult> CreateCampaign(Guid businessId, [FromBody] Campaign newCampaign)
    {
        newCampaign.BusinessId = businessId;
        var createdCampaign = await _businessService.CreateCampaignAsync(newCampaign);
        return CreatedAtAction(nameof(GetCampaignById), new { businessId = createdCampaign.BusinessId, campaignId = createdCampaign.Id }, createdCampaign);
    }
    
    [HttpGet]
    [Route("{businessId:guid}/campaigns")]
    public async Task<IActionResult> GetCampaigns(Guid businessId) => Ok(await _businessService.GetBusinessAsync(businessId));
    
    [HttpGet]
    [Route("{businessId:guid}/campaigns/{campaignId:guid}")]
    public async Task<IActionResult> GetCampaignById(Guid businessId, Guid campaignId) => Ok(await _businessService.GetBusinessAsync(businessId));
    
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