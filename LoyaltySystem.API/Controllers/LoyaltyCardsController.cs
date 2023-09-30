using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.API.Controllers;

[ApiController]
[Route("api/users/{userId:guid}/loyalty-cards")]
public class LoyaltyCardsController : ControllerBase
{
    private readonly ILoyaltyCardService _loyaltyCardService;
    public LoyaltyCardsController(ILoyaltyCardService loyaltyCardService) => _loyaltyCardService = loyaltyCardService;
    
    [HttpPost]
    public async Task<IActionResult> CreateLoyaltyCard(Guid userId, [FromBody] CreateLoyaltyCardDto dto)
    {
        var createdLoyaltyCard = await _loyaltyCardService.CreateLoyaltyCardAsync(userId, dto.BusinessId);
        return CreatedAtAction(nameof(GetLoyaltyCard), new {createdLoyaltyCard.BusinessId, userId}, createdLoyaltyCard);
    }
    
    [HttpDelete("{businessId:guid}")]
    public async Task<IActionResult> DeleteLoyaltyCard(Guid userId, Guid businessId)
    {
        await _loyaltyCardService.DeleteLoyaltyCardAsync(userId, businessId);
        // Need to make sure that we delete all data related to a Business which is being deleted (i.e. Permissions, Loyalty Cards etc)
        return NoContent();
    }

    [HttpPut("{businessId:guid}")]
    public async Task<IActionResult> UpdateLoyaltyCard(Guid userId, Guid businessId, [FromBody] UpdateLoyaltyCardDto dto)
    {
        var updatedLoyaltyCard = await _loyaltyCardService.UpdateLoyaltyCardAsync(userId, businessId, dto.Status);
        if (updatedLoyaltyCard == null) return NotFound();

        return Ok(updatedLoyaltyCard);
    }
    
    [HttpGet("{businessId:guid}")]
    public async Task<IActionResult> GetLoyaltyCard(Guid userId, Guid businessId)
    {
        try
        {
            var card = await _loyaltyCardService.GetLoyaltyCardAsync(userId, businessId);
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
}