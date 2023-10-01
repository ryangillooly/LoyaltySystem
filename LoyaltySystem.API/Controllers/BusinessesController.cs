using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.API.Controllers;

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
        if (updatedBusiness is null) return NotFound();

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
            return StatusCode(500, $"Internal server error - {ex}");
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
    public async Task<IActionResult> GetAllCampaigns(Guid businessId)
    {
        try
        {
            var campaigns = await _businessService.GetAllCampaignsAsync(businessId);
            return Ok(campaigns);
        }
        catch(ResourceNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch(Exception ex)
        {
            // Handle other exceptions as needed
            return StatusCode(500, $"Internal server error - {ex}");
        }
    }

    [HttpGet]
    [Route("{businessId:guid}/campaigns/{campaignId:guid}")]
    public async Task<IActionResult> GetCampaignById(Guid businessId, Guid campaignId)
    {
        try
        {
            var campaign = await _businessService.GetCampaignAsync(businessId, campaignId);
            return Ok(campaign);
        }
        catch(ResourceNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch(Exception ex)
        {
            // Handle other exceptions as needed
            return StatusCode(500, $"Internal server error - {ex}");
        }
    }
    
    [HttpDelete("{businessId:guid}/campaigns/{campaignId:guid}")]
    public async Task<IActionResult> DeleteCampaign(Guid businessId, Guid campaignId)
    {
        await _businessService.DeleteCampaignAsync(businessId, campaignId);
        return NoContent();
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
}