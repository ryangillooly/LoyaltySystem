namespace LoyaltySystem.Controllers;

using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Services;
using LoyaltySystem.Data;

[ApiController]
[Route("api/businesses/{businessId}/promotions")]
public class PromotionsController : ControllerBase
{
    private readonly IPromotionsService _service;

    public PromotionsController(IPromotionsService service)
    {
        _service = service;
    }

    // GET /api/businesses/{businessId}/promotions
    [HttpGet]
    public async Task<ActionResult<List<Promotion>>> GetPromotions(int businessId)
    {
        var list = await _service.GetPromotionsAsync(businessId);
        return Ok(list);
    }

    // GET /api/businesses/{businessId}/promotions/{promotionId}
    [HttpGet("{promotionId}")]
    public async Task<ActionResult<Promotion>> GetPromotion(int businessId, int promotionId)
    {
        var promo = await _service.GetPromotionAsync(businessId, promotionId);
        if (promo == null) return NotFound("Promotion not found");
        return Ok(promo);
    }

    // POST /api/businesses/{businessId}/promotions
    [HttpPost]
    public async Task<IActionResult> CreatePromotion(int businessId, [FromBody] CreatePromotionDto dto)
    {
        var newId = await _service.CreatePromotionAsync(businessId, dto);
        return Ok(new { promotionId = newId });
    }

    // PUT /api/businesses/{businessId}/promotions/{promotionId}
    [HttpPut("{promotionId}")]
    public async Task<IActionResult> UpdatePromotion(int businessId, int promotionId, [FromBody] UpdatePromotionDto dto)
    {
        await _service.UpdatePromotionAsync(businessId, promotionId, dto);
        return Ok("Promotion updated");
    }

    // DELETE /api/businesses/{businessId}/promotions/{promotionId}
    [HttpDelete("{promotionId}")]
    public async Task<IActionResult> DeletePromotion(int businessId, int promotionId)
    {
        await _service.DeletePromotionAsync(businessId, promotionId);
        return Ok("Promotion deleted");
    }
}
