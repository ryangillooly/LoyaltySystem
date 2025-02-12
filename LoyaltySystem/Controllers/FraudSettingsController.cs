namespace LoyaltySystem.Controllers;

using Microsoft.AspNetCore.Mvc;
using LoyaltySystem.Services;
using LoyaltySystem.Data;
using System.Threading.Tasks;

[ApiController]
[Route("api/stores/{storeId}/fraudsettings")]
public class FraudSettingsController : ControllerBase
{
    private readonly IFraudSettingsService _service;

    public FraudSettingsController(IFraudSettingsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<FraudSettings>> GetFraudSettings(int storeId)
    {
        var fs = await _service.GetByStoreAsync(storeId);
        if (fs == null) return NotFound("No fraud settings found for store " + storeId);
        return Ok(fs);
    }

    [HttpPost]
    public async Task<IActionResult> CreateFraudSettings(int storeId, [FromBody] CreateFraudSettingsDto dto)
    {
        // ensure dto.StoreId = storeId or pass storeId from route
        dto.StoreId = storeId;
        var newId = await _service.CreateAsync(dto);
        return Ok(new { fraudSettingId = newId });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateFraudSettings(int storeId, [FromBody] UpdateFraudSettingsDto dto)
    {
        await _service.UpdateAsync(storeId, dto);
        return Ok("Fraud settings updated");
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteFraudSettings(int storeId)
    {
        await _service.DeleteAsync(storeId);
        return Ok("Fraud settings removed");
    }
}