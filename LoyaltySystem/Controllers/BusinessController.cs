namespace LoyaltySystem.Controllers;

using Microsoft.AspNetCore.Mvc;
using LoyaltySystem.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using LoyaltySystem.Data;

[ApiController]
[Route("api/businesses")]
public class BusinessController : ControllerBase
{
    private readonly IBusinessService _businessService;

    public BusinessController(IBusinessService businessService)
    {
        _businessService = businessService;
    }

    // POST /api/businesses
    [HttpPost]
    public async Task<IActionResult> CreateBusiness([FromBody] CreateBusinessDto dto)
    {
        var newId = await _businessService.CreateBusinessAsync(dto);
        return Ok(new { businessId = newId });
    }

    // GET /api/businesses
    [HttpGet]
    public async Task<ActionResult<List<Business>>> GetAllBusinesses()
    {
        // If your user ID is in claims, you might do something like:
        // var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var userId = 0; // placeholder
        var list = await _businessService.GetAllBusinessesAsync(userId);
        return Ok(list);
    }

    // GET /api/businesses/{businessId}
    [HttpGet("{businessId}")]
    public async Task<IActionResult> GetBusiness(int businessId)
    {
        var business = await _businessService.GetBusinessByIdAsync(businessId);
        if (business == null) return NotFound("Business not found");
        return Ok(business);
    }

    // PUT /api/businesses/{businessId}
    [HttpPut("{businessId}")]
    public async Task<IActionResult> UpdateBusiness(int businessId, [FromBody] UpdateBusinessDto dto)
    {
        // Could wrap in try/catch to handle exceptions
        await _businessService.UpdateBusinessAsync(businessId, dto);
        return Ok("Business updated");
    }

    // DELETE /api/businesses/{businessId}
    [HttpDelete("{businessId}")]
    public async Task<IActionResult> DeleteBusiness(int businessId)
    {
        await _businessService.DeleteBusinessAsync(businessId);
        return Ok("Business deleted");
    }

    // ========== STAFF ==========

    // GET /api/businesses/{businessId}/staff
    [HttpGet("{businessId}/staff")]
    public async Task<IActionResult> GetStaff(int businessId)
    {
        var staffList = await _businessService.GetStaffAsync(businessId);
        return Ok(staffList);
    }

    // POST /api/businesses/{businessId}/staff
    [HttpPost("{businessId}/staff")]
    public async Task<IActionResult> AddStaff(int businessId, [FromBody] AddStaffDto dto)
    {
        await _businessService.AddStaffAsync(businessId, dto);
        return Ok("Staff added");
    }

    // PUT /api/businesses/{businessId}/staff/{businessUserId}
    [HttpPut("{businessId}/staff/{businessUserId}")]
    public async Task<IActionResult> UpdateStaff(int businessId, int businessUserId, [FromBody] UpdateStaffDto dto)
    {
        await _businessService.UpdateStaffAsync(businessUserId, dto);
        return Ok("Staff updated");
    }

    // DELETE /api/businesses/{businessId}/staff/{businessUserId}
    [HttpDelete("{businessId}/staff/{businessUserId}")]
    public async Task<IActionResult> RemoveStaff(int businessId, int businessUserId)
    {
        await _businessService.RemoveStaffAsync(businessUserId);
        return Ok("Staff removed");
    }
}
