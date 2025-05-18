using LoyaltySystem.Application.DTOs.LoyaltyPrograms;
using LoyaltySystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.Admin.API.Controllers;

[ApiController]
[Route("api/loyalty-programs/{programId}/rewards")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class RewardsController : ControllerBase
{
    private readonly ILoyaltyRewardsService _rewardsService;
    private readonly ILogger<RewardsController> _logger;

    public RewardsController(
        ILoyaltyRewardsService rewardsService,
        ILogger<RewardsController> logger)
    {
        _rewardsService = rewardsService ?? throw new ArgumentNullException(nameof(rewardsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> GetByProgramId([FromRoute] string programId)
    {
        _logger.LogInformation("Admin requesting rewards for program ID: {ProgramId}", programId);
            
        var result = await _rewardsService.GetRewardsByProgramIdAsync(programId);
            
        if (!result.Success)
        {
            _logger.LogWarning("Admin get program rewards failed for program ID: {ProgramId} - {Error}", 
                programId, result.Errors);
            return NotFound(result.Errors);
        }
            
        return Ok(result.Data);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveByProgramId(string programId)
    {
        _logger.LogInformation("Admin requesting active rewards for program ID: {ProgramId}", programId);
            
        var result = await _rewardsService.GetActiveRewardsByProgramIdAsync(programId);
            
        if (!result.Success)
        {
            _logger.LogWarning("Admin get active program rewards failed for program ID: {ProgramId} - {Error}", 
                programId, result.Errors);
            return NotFound(result.Errors);
        }
            
        return Ok(result.Data);
    }

    [HttpGet("{id}", Name="GetById")]
    public async Task<IActionResult> GetById(string id)
    {
        _logger.LogInformation("Admin requesting reward by ID: {RewardId}", id);
            
        var result = await _rewardsService.GetRewardByIdAsync(id);
            
        if (!result.Success)
        {
            _logger.LogWarning("Admin get reward failed for ID: {RewardId} - {Error}", id, result.Errors);
            return NotFound(result.Errors);
        }
            
        return Ok(result.Data);
    }

    [HttpGet("{id}/analytics")]
    public async Task<IActionResult> GetAnalytics(string id)
    {
        _logger.LogInformation("Admin requesting analytics for reward ID: {RewardId}", id);
            
        var result = await _rewardsService.GetRewardAnalyticsAsync(id);
            
        if (!result.Success)
        {
            _logger.LogWarning("Admin get reward analytics failed for ID: {RewardId} - {Error}", id, result.Errors);
            return NotFound(result.Errors);
        }
            
        return Ok(result.Data);
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetProgramRewardsAnalytics(string programId)
    {
        _logger.LogInformation("Admin requesting rewards analytics for program ID: {ProgramId}", programId);
            
        var result = await _rewardsService.GetProgramRewardsAnalyticsAsync(programId);
            
        if (!result.Success)
        {
            _logger.LogWarning("Admin get program rewards analytics failed for program ID: {ProgramId} - {Error}", 
                programId, result.Errors);
            return NotFound(result.Errors);
        }
            
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(string programId, [FromBody] CreateRewardDto request)
    {
        _logger.LogInformation("Admin creating reward for program ID: {ProgramId}", programId);
            
        try
        {
            request.Validate();
            var result = await _rewardsService.CreateRewardAsync(programId, request);
                
            if (!result.Success)
            {
                _logger.LogWarning("Admin create reward failed - {Error}", result.Errors);
                return BadRequest(result.Errors);
            }
                
            return CreatedAtAction(
                actionName: nameof(GetById),
                routeValues: new { 
                    programId = programId, 
                    id = result.Data.Id, 
                    controller = "Rewards" 
                },
                value: result.Data
            );
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Admin create reward validation failed - {Error}", ex.Message);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing loyalty program reward with the specified ID.
    /// </summary>
    /// <param name="id">The unique identifier of the reward to update.</param>
    /// <param name="request">The updated reward details.</param>
    /// <returns>An <see cref="IActionResult"/> containing the updated reward data if successful, or a BadRequest with error details if the update fails.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(string id, [FromBody] UpdateRewardDto request)
    {
        _logger.LogInformation("Admin updating reward ID: {RewardId}", id);
            
        try
        {
            // Ensure the ID in the route matches the request body
            request.Id = id;
                
            var result = await _rewardsService.UpdateRewardAsync(request);
                
            if (!result.Success)
            {
                _logger.LogWarning("Admin update reward failed for ID: {RewardId} - {Error}", id, result.Errors);
                return BadRequest(result.Errors);
            }
                
            return Ok(result.Data);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Admin update reward validation failed - {Error}", ex.Message);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a reward from a specified loyalty program.
    /// </summary>
    /// <param name="programId">The ID of the loyalty program containing the reward.</param>
    /// <param name="rewardId">The ID of the reward to delete.</param>
    /// <returns>No content if deletion is successful; otherwise, a bad request with error details.</returns>
    [HttpDelete("{rewardId}")]
    public async Task<IActionResult> DeleteAsync(string programId, string rewardId)
    {
        _logger.LogInformation("Admin deleting reward ID: {RewardId} from program ID: {ProgramId}", 
            rewardId, programId);
            
        var result = await _rewardsService.DeleteRewardAsync(programId, rewardId);
            
        if (!result.Success)
        {
            _logger.LogWarning("Admin delete reward failed for ID: {RewardId} - {Error}", rewardId, result.Errors);
            return BadRequest(result.Errors);
        }
            
        return NoContent();
    }
}