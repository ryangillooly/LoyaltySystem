using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.DTOs;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Mappers;
using LoyaltySystem.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BusinessesController : ControllerBase
{
    private readonly IBusinessService _businessService;
    public BusinessesController(IBusinessService businessService) => _businessService = businessService;

    // Businesses
    [HttpPost]
    public async Task<IActionResult> CreateBusiness([FromBody] CreateBusinessDto dto)
    {
        var newBusiness = new BusinessMapper().CreateBusinessFromCreateBusinessDto(dto);
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
            var business = await _businessService.GetBusinessAsync(businessId);
            return Ok(business);
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
    public async Task<IActionResult> GetBusinesses([FromBody] GetBusinessesDto dto)
    {
        try
        {
            var businesses = await _businessService.GetBusinessesAsync(dto.BusinessIdList);
            return Ok(businesses);
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
    
    [HttpPost("{businessId:guid}/verify-email/{token:guid}")]
    public async Task<IActionResult> VerifyEmail(Guid businessId, Guid token)
    {
        await _businessService.VerifyEmailAsync(new VerifyBusinessEmailDto(businessId, token));
        return Ok();
    }

    // Business Users
    [HttpPost("{businessId:guid}/users")]
    public async Task<IActionResult> CreateBusinessUserPermissions(Guid businessId, List<UserPermissions> newBusinessUserPermissions)
    {
        var permissionList = newBusinessUserPermissions.Select(permission => 
            new BusinessUserPermissions(businessId, permission.UserId, permission.Role)).ToList();

        var createdBusinessUsers = await _businessService.CreateBusinessUserPermissionsAsync(permissionList);
        return CreatedAtAction(nameof(GetBusinessUsersPermission), new { businessId = businessId, userId = newBusinessUserPermissions[0].UserId }, createdBusinessUsers);
    }
    
    [HttpGet("{businessId:guid}/users/{userId:guid}")]
    public async Task<IActionResult> GetBusinessUsersPermission(Guid businessId, Guid userId)
    {
        try
        {
            var businessPermissions = await _businessService.GetBusinessUsersPermissionsAsync(businessId, userId);
            return Ok(businessPermissions);
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

    [HttpGet("{businessId:guid}/users")]
    public async Task<IActionResult> GetBusinessPermissions(Guid businessId)
    {
        try
        {
            var businessPermissions = await _businessService.GetBusinessPermissionsAsync(businessId);
            return Ok(businessPermissions);
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

    [HttpPut("{businessId:guid}/users")]
    public async Task<IActionResult> UpdateBusinessUsersPermissions(Guid businessId, List<UserPermissions> updatedBusinessUserPermissions)
    {
        var permissionList = updatedBusinessUserPermissions.Select(permission =>
            new BusinessUserPermissions(businessId, permission.UserId, permission.Role)).ToList();

        var updatedBusinessUsers = await _businessService.UpdateBusinessUsersPermissionsAsync(permissionList);
        
        return Ok(updatedBusinessUsers); 
    }

    [HttpDelete("{businessId:guid}/users")]
    public async Task<IActionResult> DeleteBusinessUsersPermissions(Guid businessId, List<Guid> userIdList)
    {
        await _businessService.DeleteBusinessUsersPermissionsAsync(businessId, userIdList);
        return NoContent();
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