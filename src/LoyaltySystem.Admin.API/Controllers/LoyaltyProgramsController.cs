using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.LoyaltyPrograms;
using LoyaltySystem.Application.Services;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoyaltySystem.Admin.API.Controllers
{
    [ApiController]
    [Route("api/loyaltyprograms")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class LoyaltyProgramsController : ControllerBase
    {
        private readonly LoyaltyProgramService _programService;
        private readonly ILogger<LoyaltyProgramsController> _logger;

        public LoyaltyProgramsController(
            LoyaltyProgramService programService,
            ILogger<LoyaltyProgramsController> logger)
        {
            _programService = programService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetPrograms([FromQuery] string brandId = null, [FromQuery] string type = null, [FromQuery] int skip = 0, [FromQuery] int limit = 50)
        {
            try
            {
                if (!string.IsNullOrEmpty(brandId))
                {
                    var result = await _programService.GetProgramsByBrandIdAsync(brandId);
                    return result.Success
                        ? Ok(result.Data)
                        : BadRequest(result.Errors);
                }
                
                if (!string.IsNullOrEmpty(type))
                {
                    var result = await _programService.GetProgramsByTypeAsync(type, skip, limit);
                    return result.Success
                        ? Ok(result.Data)
                        : BadRequest(result.Errors);
                }
                else
                {
                    var result = await _programService.GetAllProgramsAsync(null, skip, limit);
                    return result.Success
                        ? Ok(result.Data)
                        : BadRequest(result.Errors);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving loyalty programs");
                return StatusCode(500, "An error occurred while retrieving loyalty programs");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProgram(string id)
        {
            try
            {
                var result = await _programService.GetProgramByIdAsync(id);
                return result.Success
                    ? Ok(result.Data)
                    : NotFound(result.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving loyalty program");
                return StatusCode(500, "An error occurred while retrieving the loyalty program");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProgram([FromBody] CreateLoyaltyProgramDto dto)
        {
            try
            {
                var result = await _programService.CreateProgramAsync(dto);
                return result.Success
                    ? CreatedAtAction(nameof(GetProgram), new { id = result.Data.Id }, result.Data)
                    : BadRequest(result.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating loyalty program");
                return StatusCode(500, "An error occurred while creating the loyalty program");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProgram(string id, [FromBody] UpdateLoyaltyProgramDto dto)
        {
            try
            {
                var result = await _programService.UpdateProgramAsync(id, dto);
                return result.Success
                    ? Ok(result.Data)
                    : BadRequest(result.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating loyalty program");
                return StatusCode(500, "An error occurred while updating the loyalty program");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProgram(string id)
        {
            try
            {
                var result = await _programService.DeleteProgramAsync(id);
                return result.Success
                    ? NoContent()
                    : BadRequest(result.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting loyalty program");
                return StatusCode(500, "An error occurred while deleting the loyalty program");
            }
        }

        [HttpPost("{programId}/rewards")]
        public async Task<IActionResult> AddReward(string programId, [FromBody] CreateRewardDto dto)
        {
            try
            {
                var result = await _programService.AddRewardToProgramAsync(programId, dto);
                return result.Success
                    ? Created($"/api/admin/loyalty-programs/{programId}/rewards/{result.Data.Id}", result.Data)
                    : BadRequest(result.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding reward to loyalty program");
                return StatusCode(500, "An error occurred while adding the reward");
            }
        }

        [HttpPut("{programId}/rewards/{rewardId}")]
        public async Task<IActionResult> UpdateReward(string programId, string rewardId, [FromBody] UpdateRewardDto dto)
        {
            try
            {
                dto.Id = rewardId;
                dto.ProgramId = programId;
                var result = await _programService.UpdateRewardAsync(dto);
                return result.Success
                    ? Ok(result.Data)
                    : BadRequest(result.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reward");
                return StatusCode(500, "An error occurred while updating the reward");
            }
        }

        [HttpDelete("{programId}/rewards/{rewardId}")]
        public async Task<IActionResult> RemoveReward(string programId, string rewardId)
        {
            try
            {
                var result = await _programService.RemoveRewardFromProgramAsync(programId, rewardId);
                return result.Success
                    ? NoContent()
                    : BadRequest(new { error = result.Errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing reward from loyalty program");
                return StatusCode(500, "An error occurred while removing the reward");
            }
        }

        [HttpGet("analytics")]
        public async Task<IActionResult> GetProgramAnalytics()
        {
            _logger.LogInformation("Admin requesting loyalty program analytics");
            
            var result = await _programService.GetProgramAnalyticsAsync();
            
            if (!result.Success)
            {
                _logger.LogWarning("Get program analytics failed - {Error}", result.Errors);
                return BadRequest(result.Errors);
            }
            
            return Ok(result.Data);
        }

        [HttpGet("{id}/analytics")]
        public async Task<IActionResult> GetProgramDetailedAnalytics(LoyaltyProgramId id)
        {
            _logger.LogInformation("Admin requesting detailed analytics for program ID: {ProgramId}", id);
            
            // First verify the program exists
            var programResult = await _programService.GetProgramByIdAsync(id.ToString());
            if (!programResult.Success)
            {
                _logger.LogWarning("Program ID: {ProgramId} not found", id);
                return NotFound("Program not found");
            }
            
            var result = await _programService.GetProgramDetailedAnalyticsAsync(id.ToString());
            
            if (!result.Success)
            {
                _logger.LogWarning("Get detailed program analytics failed for ID: {ProgramId} - {Error}", 
                    id, result.Errors);
                return BadRequest(result.Errors);
            }
            
            return Ok(result.Data);
        }

        public class CreateProgramDto
        {
            public Guid BrandId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Type { get; set; }
            public ExpirationPolicyRequestDto ExpirationPolicy { get; set; }
        }
        
        public class ExpirationPolicyRequestDto
        {
            public bool HasExpiration { get; set; }
            public bool ExpiresOnSpecificDate { get; set; }
            public int? ExpirationType { get; set; }
            public int? ExpirationValue { get; set; }
            public int? ExpirationDay { get; set; }
            public int? ExpirationMonth { get; set; }
            public DateTime? ExpirationDate { get; set; }
            public int? DurationInDays { get; set; }
        }
    }

    public class UpdateProgramRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public ExpirationPolicyDto ExpirationPolicy { get; set; }
        public int? StampThreshold { get; set; }
        public decimal? PointsConversionRate { get; set; }
        public int? DailyStampLimit { get; set; }
        public decimal? MinimumTransactionAmount { get; set; }
        
        // Points configuration
        public PointsConfigDto PointsConfig { get; set; }
        
        // Tiers configuration
        public List<TierDto> Tiers { get; set; }
        public bool? HasTiers { get; set; }
    }

    public class CreateRewardRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int? RequiredPoints { get; set; }
        public int? RequiredStamps { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? AvailableQuantity { get; set; }
    }

    public class UpdateRewardRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int? RequiredPoints { get; set; }
        public int? RequiredStamps { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? AvailableQuantity { get; set; }
    }
} 