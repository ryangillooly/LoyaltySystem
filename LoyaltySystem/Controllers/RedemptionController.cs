namespace LoyaltySystem.Controllers;

using Microsoft.AspNetCore.Mvc;
using LoyaltySystem.Services;
using LoyaltySystem.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/businesses/{businessId}/redemptions")]
public class RedemptionsController : ControllerBase
{
    private readonly IRedemptionService _service;

    public RedemptionsController(IRedemptionService service)
    {
        _service = service;
    }

    // GET /api/businesses/{businessId}/redemptions
    [HttpGet]
    public async Task<ActionResult<List<RedemptionTransaction>>> GetRedemptions(int businessId)
    {
        var list = await _service.GetRedemptionsAsync(businessId);
        return Ok(list);
    }

    // GET /api/businesses/{businessId}/redemptions/{redemptionId}
    [HttpGet("{redemptionId}")]
    public async Task<ActionResult<RedemptionTransaction>> GetRedemption(int businessId, int redemptionId)
    {
        var redemption = await _service.GetRedemptionAsync(businessId, redemptionId);
        if (redemption == null) return NotFound("Redemption not found");
        return Ok(redemption);
    }

    // POST /api/businesses/{businessId}/redemptions
    [HttpPost]
    public async Task<IActionResult> CreateRedemption(int businessId, [FromBody] CreateRedemptionDto dto)
    {
        var newId = await _service.CreateRedemptionAsync(businessId, dto);
        return Ok(new { redemptionTransactionId = newId });
    }

    // PUT /api/businesses/{businessId}/redemptions/{redemptionId}
    [HttpPut("{redemptionId}")]
    public async Task<IActionResult> UpdateRedemption(int businessId, int redemptionId, [FromBody] UpdateRedemptionDto dto)
    {
        await _service.UpdateRedemptionAsync(businessId, redemptionId, dto);
        return Ok("Redemption updated");
    }

    // DELETE /api/businesses/{businessId}/redemptions/{redemptionId}
    [HttpDelete("{redemptionId}")]
    public async Task<IActionResult> DeleteRedemption(int businessId, int redemptionId)
    {
        await _service.DeleteRedemptionAsync(businessId, redemptionId);
        return Ok("Redemption deleted");
    }
}
