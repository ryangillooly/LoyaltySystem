namespace LoyaltySystem.Controllers;

using Microsoft.AspNetCore.Mvc;
using LoyaltySystem.Services;
using System.Threading.Tasks;

[ApiController]
[Route("api/businesses/{businessId}/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _service;

    public AnalyticsController(IAnalyticsService service)
    {
        _service = service;
    }

    // GET /api/businesses/{businessId}/analytics/summary
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(int businessId)
    {
        var result = await _service.GetSummaryAsync(businessId);
        return Ok(result);
    }

    // GET /api/businesses/{businessId}/analytics/activity
    [HttpGet("activity")]
    public async Task<IActionResult> GetActivity(int businessId, [FromQuery] string range = "7d")
    {
        // E.g. get daily or weekly aggregates
        var result = await _service.GetActivityAsync(businessId, range);
        return Ok(result);
    }

    // GET /api/businesses/{businessId}/analytics/stamp-breakdown
    [HttpGet("stamp-breakdown")]
    public async Task<IActionResult> GetStampBreakdown(int businessId, [FromQuery] string by = "store")
    {
        // E.g. stamps by store or loyalty card
        var result = await _service.GetStampBreakdownAsync(businessId, by);
        return Ok(result);
    }

    // GET /api/businesses/{businessId}/analytics/redemptions-breakdown
    [HttpGet("redemptions-breakdown")]
    public async Task<IActionResult> GetRedemptionsBreakdown(int businessId, [FromQuery] string by = "reward")
    {
        // E.g. redemptions by store or by reward
        var result = await _service.GetRedemptionsBreakdownAsync(businessId, by);
        return Ok(result);
    }

    // GET /api/businesses/{businessId}/reports
    [HttpGet("/api/businesses/{businessId}/reports")]
    public async Task<IActionResult> GenerateReport(int businessId)
    {
        // Possibly generate CSV or PDF or just JSON
        var report = await _service.GenerateReportAsync(businessId);
        return Ok(report);
    }
}
