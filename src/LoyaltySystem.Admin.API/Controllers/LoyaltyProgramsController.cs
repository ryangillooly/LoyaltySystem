using System;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Services;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoyaltySystem.Admin.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class LoyaltyProgramsController : ControllerBase
    {
        private readonly LoyaltyProgramService _programService;
        private readonly ILogger<LoyaltyProgramsController> _logger;

        public LoyaltyProgramsController(
            LoyaltyProgramService programService,
            ILogger<LoyaltyProgramsController> logger)
        {
            _programService = programService ?? throw new ArgumentNullException(nameof(programService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPrograms([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            _logger.LogInformation("Admin requesting all loyalty programs (page {Page}, size {PageSize})", 
                page, pageSize);
            
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Invalid pagination parameters");
            }
            
            var result = await _programService.GetAllProgramsAsync(page, pageSize);
            
            if (!result.Success)
            {
                _logger.LogWarning("Get all loyalty programs failed - {Error}", result.Errors);
                return BadRequest(result.Errors);
            }
            
            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(LoyaltyProgramId id)
        {
            _logger.LogInformation("Admin requesting loyalty program by ID: {ProgramId}", id);
            
            var result = await _programService.GetProgramByIdAsync(id.ToString());
            
            if (!result.Success)
            {
                _logger.LogWarning("Get loyalty program failed for ID: {ProgramId} - {Error}", id, result.Errors);
                return NotFound(result.Errors);
            }
            
            return Ok(result.Data);
        }

        [HttpGet("brand/{brandId}")]
        public async Task<IActionResult> GetByBrandId(Guid brandId)
        {
            _logger.LogInformation("Admin requesting loyalty programs for brand ID: {BrandId}", brandId);
            
            var result = await _programService.GetProgramsByBrandIdAsync(brandId.ToString());
            
            if (!result.Success)
            {
                _logger.LogWarning("Get brand loyalty programs failed for brand ID: {BrandId} - {Error}", 
                    brandId, result.Errors);
                return NotFound(result.Errors);
            }
            
            return Ok(result.Data);
        }

        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetByType(string type, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            _logger.LogInformation("Admin requesting loyalty programs of type: {Type} (page {Page}, size {PageSize})", 
                type, page, pageSize);
            
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Invalid pagination parameters");
            }
            
            var result = await _programService.GetProgramsByTypeAsync(type, page, pageSize);
            
            if (!result.Success)
            {
                _logger.LogWarning("Get loyalty programs by type failed for type: {Type} - {Error}", 
                    type, result.Errors);
                return BadRequest(result.Errors);
            }
            
            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProgram([FromBody] CreateProgramRequest request)
        {
            _logger.LogInformation("Admin creating loyalty program for brand ID: {BrandId}", request.BrandId);
            
            var createDto = new CreateLoyaltyProgramDto
            {
                BrandId = request.BrandId.ToString(),
                Name = request.Name,
                Type = Enum.Parse<LoyaltyProgramType>(request.Type),
                StampThreshold = request.ExpirationPolicy?.PeriodMonths,
                PointsConversionRate = null,
                DailyStampLimit = null,
                MinimumTransactionAmount = null,
                HasExpiration = request.ExpirationPolicy != null,
                ExpiresOnSpecificDate = request.ExpirationPolicy?.ExpirationDate != null,
                ExpirationType = null,
                ExpirationValue = null,
                ExpirationDay = request.ExpirationPolicy?.ExpirationDate?.Day,
                ExpirationMonth = request.ExpirationPolicy?.ExpirationDate?.Month
            };
            
            var result = await _programService.CreateProgramAsync(createDto);
            
            if (!result.Success)
            {
                _logger.LogWarning("Admin create loyalty program failed - {Error}", result.Errors);
                return BadRequest(result.Errors);
            }
            
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProgram([FromRoute] LoyaltyProgramId id, [FromBody] UpdateProgramRequest request)
        {
            _logger.LogInformation("Admin updating loyalty program ID: {ProgramId}", id);
            
            var updateDto = new UpdateLoyaltyProgramDto
            {
                Name = request.Name,
                StampThreshold = null,
                PointsConversionRate = null,
                DailyStampLimit = null,
                MinimumTransactionAmount = null,
                HasExpiration = request.ExpirationPolicy != null,
                ExpiresOnSpecificDate = request.ExpirationPolicy?.ExpirationDate != null,
                ExpirationType = null,
                ExpirationValue = null,
                ExpirationDay = request.ExpirationPolicy?.ExpirationDate?.Day,
                ExpirationMonth = request.ExpirationPolicy?.ExpirationDate?.Month
            };
            
            var result = await _programService.UpdateProgramAsync(id.ToString(), updateDto);
            
            if (!result.Success)
            {
                _logger.LogWarning("Admin update loyalty program failed for ID: {ProgramId} - {Error}", 
                    id, result.Errors);
                return BadRequest(result.Errors);
            }
            
            return Ok(result.Data);
        }

        [HttpPost("{id}/rewards")]
        public async Task<IActionResult> AddReward([FromRoute] LoyaltyProgramId id, [FromBody] CreateRewardRequest request)
        {
            _logger.LogInformation("Admin adding reward to program ID: {ProgramId}", id);
            
            var createDto = new CreateRewardDto
            {
                Title = request.Name,
                Description = request.Description,
                RequiredPoints = request.RequiredPoints ?? 0,
                RequiredStamps = request.RequiredStamps ?? 0,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };
            
            var result = await _programService.CreateRewardAsync(id.ToString(), createDto);
            
            if (!result.Success)
            {
                _logger.LogWarning("Admin add reward failed for program ID: {ProgramId} - {Error}", 
                    id, result.Errors);
                return BadRequest(result.Errors);
            }
            
            return CreatedAtAction(nameof(GetReward), new { id = id, rewardId = result.Data.Id }, result.Data);
        }

        [HttpGet("{id}/rewards")]
        public async Task<IActionResult> GetRewards(LoyaltyProgramId id)
        {
            _logger.LogInformation("Admin requesting rewards for program ID: {ProgramId}", id);
            
            var result = await _programService.GetRewardsByProgramIdAsync(id.ToString());
            
            if (!result.Success)
            {
                _logger.LogWarning("Get program rewards failed for program ID: {ProgramId} - {Error}", 
                    id, result.Errors);
                return NotFound(result.Errors);
            }
            
            return Ok(result.Data);
        }

        [HttpGet("{id}/rewards/{rewardId}")]
        public async Task<IActionResult> GetReward(LoyaltyProgramId id, Guid rewardId)
        {
            _logger.LogInformation("Admin requesting reward ID: {RewardId} for program ID: {ProgramId}", 
                rewardId, id);
            
            var result = await _programService.GetRewardByIdAsync(rewardId.ToString());
            
            if (!result.Success)
            {
                _logger.LogWarning("Get reward failed for ID: {RewardId} - {Error}", rewardId, result.Errors);
                return NotFound(result.Errors);
            }
            
            // Verify reward belongs to this program
            if (result.Data.ProgramId != id.ToString())
            {
                _logger.LogWarning("Reward ID: {RewardId} does not belong to program ID: {ProgramId}", 
                    rewardId, id);
                return NotFound();
            }
            
            return Ok(result.Data);
        }

        [HttpPut("{id}/rewards/{rewardId}")]
        public async Task<IActionResult> UpdateReward([FromRoute] LoyaltyProgramId id, [FromRoute] Guid rewardId, [FromBody] UpdateRewardRequest request)
        {
            _logger.LogInformation("Admin updating reward ID: {RewardId} in program ID: {ProgramId}", rewardId, id);
            
            // First verify reward exists and belongs to this program
            var getResult = await _programService.GetRewardByIdAsync(rewardId.ToString());
            if (!getResult.Success)
            {
                _logger.LogWarning("Reward ID: {RewardId} not found", rewardId);
                return NotFound();
            }
            
            if (getResult.Data.ProgramId != id.ToString())
            {
                _logger.LogWarning("Reward ID: {RewardId} does not belong to program ID: {ProgramId}", 
                    rewardId, id);
                return NotFound();
            }
            
            var updateDto = new UpdateRewardDto
            {
                Title = request.Name,
                Description = request.Description,
                RequiredPoints = request.RequiredPoints ?? 0,
                RequiredStamps = request.RequiredStamps ?? 0,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };
            
            var result = await _programService.UpdateRewardAsync(rewardId.ToString(), updateDto);
            
            if (!result.Success)
            {
                _logger.LogWarning("Admin update reward failed for ID: {RewardId} - {Error}", 
                    rewardId, result.Errors);
                return BadRequest(result.Errors);
            }
            
            return Ok(result.Data);
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
    }

    public class CreateProgramRequest
    {
        public Guid BrandId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public ExpirationPolicyDto ExpirationPolicy { get; set; }
    }

    public class UpdateProgramRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public ExpirationPolicyDto ExpirationPolicy { get; set; }
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