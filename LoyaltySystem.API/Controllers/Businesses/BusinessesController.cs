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
    
    [HttpGet("{businessId}")]
    public async Task<IActionResult> GetBusiness(Guid businessId) => Ok(await _businessService.GetByIdAsync(businessId));
}