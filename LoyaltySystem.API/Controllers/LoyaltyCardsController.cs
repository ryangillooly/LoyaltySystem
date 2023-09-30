using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.Controllers;

[ApiController]
[Route("api/users/{userId:guid}/loyalty-cards")]
public class LoyaltyCardsController : ControllerBase
{
    private readonly ILoyaltyCardService _loyaltyCardService;
    public LoyaltyCardsController(ILoyaltyCardService loyaltyCardService) => _loyaltyCardService = loyaltyCardService;
    
    [HttpPost]
    public async Task<IActionResult> CreateLoyaltyCard(Guid userId, [FromBody] LoyaltyCardDto dto)
    {
        var createdLoyaltyCard = await _loyaltyCardService.CreateLoyaltyCardAsync(userId, dto.BusinessId);
        return CreatedAtAction(nameof(GetLoyaltyCard), new {createdLoyaltyCard.BusinessId, userId}, createdLoyaltyCard);
    }
    
    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetLoyaltyCard(Guid businessId, Guid userId) => Ok(await _loyaltyCardService.GetByIdAsync(businessId, userId));
}