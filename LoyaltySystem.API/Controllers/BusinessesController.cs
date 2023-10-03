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

    // Businesses
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

    // Business Users
    [HttpPost("{businessId:guid}/users")]
    public async Task<IActionResult> CreateBusinessUserPermissions(Guid businessId, BusinessUserPermissions newBusinessUserPermissions)
    {
        newBusinessUserPermissions.BusinessId = businessId;
        var createdBusinessUsers = await _businessService.CreateBusinessUserPermissionsAsync(newBusinessUserPermissions);
        return CreatedAtAction(nameof(GetBusinessUserPermission), new { businessId = businessId, userId = newBusinessUserPermissions.Permissions[0].UserId }, createdBusinessUsers);
    }
    
    [HttpGet("{businessId:guid}/users/{userId:guid}")]
    public async Task<IActionResult> GetBusinessUserPermission(Guid businessId, Guid userId) => throw new NotImplementedException();
    
    [HttpGet("{businessId:guid}/users")]
    public async Task<IActionResult> GetBusinessUsers(Guid businessId, User user) => throw new NotImplementedException();
    
    [HttpPut("{businessId:guid}/users/{userId:guid}")]
    public async Task<IActionResult> UpdateBusinessUser(Guid businessId, Guid userId, User user) => throw new NotImplementedException();
    
    [HttpPut("{businessId:guid}/users/{userId:guid}")]
    public async Task<IActionResult> DeleteBusinessUser(Guid businessId, Guid userId) => throw new NotImplementedException();


    // Permissions
    [HttpPost]
    [HttpPut]
    [Route("{businessId:guid}/users")]
    public async Task<bool> PutUserPermission(Guid businessId, [FromBody] List<BusinessUserPermissions> permissions)
    {
        var permissionList = new List<BusinessUserPermissions>();
            
        foreach (var permission in permissions)
        {
            permissionList.Add
            (
                new BusinessUserPermissions
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
    
    
    // Campaigns
    [HttpPost("{businessId:guid}/campaigns")]
    public async Task<IActionResult> CreateCampaign(Guid businessId, [FromBody] Campaign newCampaign)
    {
        newCampaign.BusinessId = businessId;
        var createdCampaign = await _businessService.CreateCampaignAsync(newCampaign);
        return CreatedAtAction(nameof(GetCampaign), new { businessId = createdCampaign.BusinessId, campaignId = createdCampaign.Id }, createdCampaign);
    }
    [HttpGet("{businessId:guid}/campaigns")]
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
    [HttpGet("{businessId:guid}/campaigns/{campaignId:guid}")]
    public async Task<IActionResult> GetCampaign(Guid businessId, Guid campaignId)
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
    [HttpPut("{businessId:guid}/campaigns/{campaignId:guid}")]
    public async Task<IActionResult> UpdateCampaign(Guid businessId, Guid campaignId, [FromBody] Campaign campaign)
    {
        campaign.Id         = campaignId;
        campaign.BusinessId = businessId;
        var updatedCampaign = await _businessService.UpdateCampaignAsync(campaign);
        if (updatedCampaign is null) return NotFound();

        return Ok(updatedCampaign);   
    }
    [HttpDelete("{businessId:guid}/campaigns")]
    public async Task<IActionResult> DeleteCampaigns(Guid businessId, List<Guid> campaignIds)
    {
        await _businessService.DeleteCampaignAsync(businessId, campaignIds);
        return NoContent();
    }
}