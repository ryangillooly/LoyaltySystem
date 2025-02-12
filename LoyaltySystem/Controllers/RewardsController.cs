namespace LoyaltySystem.Controllers;

using Microsoft.AspNetCore.Mvc;
using LoyaltySystem.Services;
using LoyaltySystem.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/businesses/{businessId}/rewards")]
public class RewardsController : ControllerBase
{
    private readonly IRewardsService _service;

    public RewardsController(IRewardsService service)
    {
        _service = service;
    }

    // GET /api/businesses/{businessId}/rewards
    [HttpGet]
    public async Task<ActionResult<List<Reward>>> GetRewards(int businessId)
    {
        var list = await _service.GetRewardsAsync(businessId);
        return Ok(list);
    }

    // GET /api/businesses/{businessId}/rewards/{rewardId}
    [HttpGet("{rewardId}")]
    public async Task<ActionResult<Reward>> GetReward(int businessId, int rewardId)
    {
        var reward = await _service.GetRewardAsync(businessId, rewardId);
        if (reward == null) return NotFound("Reward not found");
        return Ok(reward);
    }

    // POST /api/businesses/{businessId}/rewards
    [HttpPost]
    public async Task<IActionResult> CreateReward(int businessId, [FromBody] CreateRewardDto dto)
    {
        var newId = await _service.CreateRewardAsync(businessId, dto);
        return Ok(new { rewardId = newId });
    }

    // PUT /api/businesses/{businessId}/rewards/{rewardId}
    [HttpPut("{rewardId}")]
    public async Task<IActionResult> UpdateReward(int businessId, int rewardId, [FromBody] UpdateRewardDto dto)
    {
        await _service.UpdateRewardAsync(businessId, rewardId, dto);
        return Ok("Reward updated");
    }

    // DELETE /api/businesses/{businessId}/rewards/{rewardId}
    [HttpDelete("{rewardId}")]
    public async Task<IActionResult> DeleteReward(int businessId, int rewardId)
    {
        await _service.DeleteRewardAsync(businessId, rewardId);
        return Ok("Reward deleted");
    }
}