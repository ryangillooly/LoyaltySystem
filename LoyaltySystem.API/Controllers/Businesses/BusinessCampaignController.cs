using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.Controllers;

[ApiController]
[Route("api/businesses/{businessId:guid}/campaigns")]
public class BusinessCampaignController : ControllerBase
{
    private readonly IBusinessService _businessService;
    public BusinessCampaignController(IBusinessService businessService) => _businessService = businessService;
    
    [HttpPost]
    public async Task<IActionResult> CreateCampaign(Guid businessId, [FromBody] Campaign newCampaign)
    {
        newCampaign.BusinessId = businessId;
        var createdCampaign = await _businessService.CreateCampaignAsync(newCampaign);
        return CreatedAtAction(nameof(GetCampaignById), new { id = createdCampaign.Id }, newCampaign);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetCampaigns(Guid businessId) => Ok(await _businessService.GetByIdAsync(businessId));
    
    [HttpGet]
    [Route("{campaignId:guid}")]
    public async Task<IActionResult> GetCampaignById(Guid businessId, Guid campaignId) => Ok(await _businessService.GetByIdAsync(businessId));
}