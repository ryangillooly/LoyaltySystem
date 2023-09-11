using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.Controllers;

[ApiController]
[Route("api/users/{userEmail}/loyalty-cards")]
public class LoyaltyController : ControllerBase
{
    private readonly ILoyaltyCardService _loyaltyCardService;
    public LoyaltyController(ILoyaltyCardService loyaltyCardService) => _loyaltyCardService = loyaltyCardService;
    
    [HttpPost]
    public async Task<IActionResult> CreateLoyaltyCard([FromBody] LoyaltyCard newLoyaltyCard, string userEmail)
    {
        newLoyaltyCard.UserEmail = userEmail;
        var createdLoyaltyCard = await _loyaltyCardService.CreateAsync(newLoyaltyCard);
        return CreatedAtAction(nameof(GetLoyaltyCard), new { userEmail, businessId = createdLoyaltyCard.BusinessId }, createdLoyaltyCard);
    }
    
    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetLoyaltyCard(Guid businessId, string userEmail) => Ok(await _loyaltyCardService.GetByIdAsync(businessId, userEmail));
}