namespace LoyaltySystem.Controllers;

using Microsoft.AspNetCore.Mvc;
using LoyaltySystem.Services;
using LoyaltySystem.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/businesses/{businessId}/stores")]
public class StoresController : ControllerBase
{
    private readonly IStoresService _storesService;

    public StoresController(IStoresService storesService)
    {
        _storesService = storesService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Store>>> GetStores(int businessId)
    {
        var stores = await _storesService.GetStoresAsync(businessId);
        return Ok(stores);
    }

    [HttpGet("{storeId}")]
    public async Task<ActionResult<Store>> GetStore(int businessId, int storeId)
    {
        var store = await _storesService.GetStoreAsync(businessId, storeId);
        if (store == null) return NotFound();
        return Ok(store);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStore(int businessId, [FromBody] CreateStoreDto dto)
    {
        var newId = await _storesService.CreateStoreAsync(businessId, dto);
        return Ok(new { storeId = newId });
    }

    [HttpPut("{storeId}")]
    public async Task<IActionResult> UpdateStore(int businessId, int storeId, [FromBody] UpdateStoreDto dto)
    {
        await _storesService.UpdateStoreAsync(businessId, storeId, dto);
        return Ok("Store updated");
    }

    [HttpDelete("{storeId}")]
    public async Task<IActionResult> DeleteStore(int businessId, int storeId)
    {
        await _storesService.DeleteStoreAsync(businessId, storeId);
        return Ok("Store deleted");
    }
}