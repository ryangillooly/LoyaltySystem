using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.Controllers;

[ApiController]
[Route("api/users/{userId:guid}/loyalty-cards")]
public class LoyaltyController : ControllerBase
{
    private readonly ILoyaltyCardService _loyaltyCardService;
    public LoyaltyController(ILoyaltyCardService loyaltyCardService) => _loyaltyCardService = loyaltyCardService;
    
    [HttpPost]
    public async Task<IActionResult> CreateLoyaltyCard(Guid userId, [FromBody] LoyaltyCard newLoyaltyCard)
    {
        newLoyaltyCard.UserId = userId;
        var createdLoyaltyCard = await _loyaltyCardService.CreateAsync(newLoyaltyCard);
        return CreatedAtAction(nameof(GetLoyaltyCard), new {createdLoyaltyCard.BusinessId, userId}, createdLoyaltyCard);
    }
    
    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetLoyaltyCard(Guid businessId, Guid userId) => Ok(await _loyaltyCardService.GetByIdAsync(businessId, userId));
}