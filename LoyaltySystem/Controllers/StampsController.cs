namespace LoyaltySystem.Controllers;

using Microsoft.AspNetCore.Mvc;
using LoyaltySystem.Services;
using LoyaltySystem.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/businesses/{businessId}/stamps")]
public class StampsController : ControllerBase
{
    private readonly IStampsService _service;

    public StampsController(IStampsService service)
    {
        _service = service;
    }

    // GET /api/businesses/{businessId}/stamps
    [HttpGet]
    public async Task<ActionResult<List<StampTransaction>>> GetStamps(int businessId)
    {
        var list = await _service.GetStampsAsync(businessId);
        return Ok(list);
    }

    // GET /api/businesses/{businessId}/stamps/{stampTransactionId}
    [HttpGet("{stampTransactionId}")]
    public async Task<ActionResult<StampTransaction>> GetStampTransaction(int businessId, int stampTransactionId)
    {
        var stamp = await _service.GetStampTransactionAsync(businessId, stampTransactionId);
        if (stamp == null) return NotFound("Stamp transaction not found");
        return Ok(stamp);
    }

    // POST /api/businesses/{businessId}/stamps
    [HttpPost]
    public async Task<IActionResult> CreateStamp(int businessId, [FromBody] CreateStampDto dto)
    {
        var newId = await _service.CreateStampAsync(businessId, dto);
        return Ok(new { stampTransactionId = newId });
    }

    // PUT /api/businesses/{businessId}/stamps/{stampTransactionId}
    [HttpPut("{stampTransactionId}")]
    public async Task<IActionResult> UpdateStamp(int businessId, int stampTransactionId, [FromBody] UpdateStampDto dto)
    {
        await _service.UpdateStampTransactionAsync(businessId, stampTransactionId, dto);
        return Ok("Stamp transaction updated");
    }

    // DELETE /api/businesses/{businessId}/stamps/{stampTransactionId}
    [HttpDelete("{stampTransactionId}")]
    public async Task<IActionResult> DeleteStamp(int businessId, int stampTransactionId)
    {
        await _service.DeleteStampTransactionAsync(businessId, stampTransactionId);
        return Ok("Stamp transaction deleted");
    }
}
