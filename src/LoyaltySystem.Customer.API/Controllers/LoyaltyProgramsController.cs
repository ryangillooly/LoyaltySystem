using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.LoyaltyPrograms;
using LoyaltySystem.Application.Services;
using LoyaltySystem.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace LoyaltySystem.Customer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoyaltyProgramsController : ControllerBase
    {
        private readonly LoyaltyProgramService _programService;
        private readonly BrandService _brandService;
        private readonly ILogger<LoyaltyProgramsController> _logger;

        public LoyaltyProgramsController(
            LoyaltyProgramService programService,
            BrandService brandService,
            ILogger<LoyaltyProgramsController> logger)
        {
            _programService = programService ?? throw new ArgumentNullException(nameof(programService));
            _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailablePrograms([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string brandId = null)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("User identity not found when requesting available loyalty programs");
                return Unauthorized();
            }

            _logger.LogInformation("Customer user {UserId} requesting available loyalty programs (page {Page}, size {PageSize})", 
                userIdClaim, page, pageSize);
            
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Invalid pagination parameters");
            }
            
            // Get only active programs for customers
            var result = await _programService.GetAllProgramsAsync(brandId, page, pageSize);
            
            if (!result.Success)
            {
                _logger.LogWarning("Get available loyalty programs failed - {Error}", result.Errors);
                return BadRequest(result.Errors);
            }
            
            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(LoyaltyProgramId id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("User identity not found when requesting loyalty program details");
                return Unauthorized();
            }

            _logger.LogInformation("Customer user {UserId} requesting loyalty program ID: {ProgramId}", 
                userIdClaim, id);
            
            var result = await _programService.GetProgramByIdAsync(id.ToString());
            
            if (!result.Success)
            {
                _logger.LogWarning("Get loyalty program failed for ID: {ProgramId} - {Error}", id, result.Errors);
                return NotFound(result.Errors);
            }
            
            // Only show active programs to customers
            if (!result.Data.IsActive)
            {
                _logger.LogWarning("Customer attempted to access inactive program ID: {ProgramId}", id);
                return NotFound("Program not found");
            }
            
            // Include brand information with the program
            var brandResult = await _brandService.GetBrandByIdAsync(result.Data.BrandId);
            if (brandResult.Success)
            {
                return Ok(new
                {
                    Program = result.Data,
                    Brand = new
                    {
                        brandResult.Data.Id,
                        brandResult.Data.Name,
                        LogoUrl = brandResult.Data.Logo,
                        WebsiteUrl = brandResult.Data.ContactInfo?.Website ?? string.Empty
                    }
                });
            }
            
            return Ok(result.Data);
        }

        [HttpGet("brand/{brandId}")]
        public async Task<IActionResult> GetByBrandId(Guid brandId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("User identity not found when requesting brand loyalty programs");
                return Unauthorized();
            }

            _logger.LogInformation("Customer user {UserId} requesting loyalty programs for brand ID: {BrandId}", 
                userIdClaim, brandId);
            
            var result = await _programService.GetProgramsByBrandIdAsync(brandId.ToString());
            
            if (!result.Success)
            {
                _logger.LogWarning("Get brand loyalty programs failed for brand ID: {BrandId} - {Error}", 
                    brandId, result.Errors);
                return NotFound(result.Errors);
            }
            
            return Ok(result.Data);
        }

        [HttpGet("{id}/rewards")]
        public async Task<IActionResult> GetRewards(LoyaltyProgramId id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("User identity not found when requesting program rewards");
                return Unauthorized();
            }

            _logger.LogInformation("Customer user {UserId} requesting rewards for program ID: {ProgramId}", 
                userIdClaim, id);
            
            // First verify the program exists and is active
            var programResult = await _programService.GetProgramByIdAsync(id.ToString());
            if (!programResult.Success)
            {
                _logger.LogWarning("Program ID: {ProgramId} not found", id);
                return NotFound("Program not found");
            }
            
            if (!programResult.Data.IsActive)
            {
                _logger.LogWarning("Customer attempted to access rewards for inactive program ID: {ProgramId}", id);
                return NotFound("Program not found");
            }
            
            // Only return active rewards for customers
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
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("User identity not found when requesting reward details");
                return Unauthorized();
            }

            _logger.LogInformation("Customer user {UserId} requesting reward ID: {RewardId} for program ID: {ProgramId}", 
                userIdClaim, rewardId, id);
            
            // First verify the program exists and is active
            var programResult = await _programService.GetProgramByIdAsync(id.ToString());
            if (!programResult.Success)
            {
                _logger.LogWarning("Program ID: {ProgramId} not found", id);
                return NotFound("Program not found");
            }
            
            if (!programResult.Data.IsActive)
            {
                _logger.LogWarning("Customer attempted to access reward for inactive program ID: {ProgramId}", id);
                return NotFound("Program not found");
            }
            
            // Get all rewards for the program
            var rewardsResult = await _programService.GetRewardsByProgramIdAsync(id.ToString());
            
            if (!rewardsResult.Success)
            {
                _logger.LogWarning("Get rewards failed for program ID: {ProgramId} - {Error}", 
                    id, rewardsResult.Errors);
                return NotFound(rewardsResult.Errors);
            }
            
            // Find the specific reward by ID
            var reward = rewardsResult.Data.Find(r => r.Id == rewardId.ToString());
            if (reward == null)
            {
                _logger.LogWarning("Reward ID: {RewardId} not found in program ID: {ProgramId}", 
                    rewardId, id);
                return NotFound("Reward not found");
            }
            
            // Verify reward is active
            if (!reward.IsActive)
            {
                _logger.LogWarning("Customer attempted to access inactive reward ID: {RewardId}", rewardId);
                return NotFound("Reward not found");
            }
            
            return Ok(reward);
        }

        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearbyPrograms(
            [FromQuery] double latitude, 
            [FromQuery] double longitude, 
            [FromQuery] double radiusKm = 10,
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("User identity not found when requesting nearby loyalty programs");
                return Unauthorized();
            }

            _logger.LogInformation("Customer user {UserId} requesting programs near location ({Latitude}, {Longitude})", 
                userIdClaim, latitude, longitude);
            
            // Validate inputs
            if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
            {
                return BadRequest("Invalid coordinates");
            }
            
            if (radiusKm <= 0 || radiusKm > 100)
            {
                return BadRequest("Invalid radius");
            }
            
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Invalid pagination parameters");
            }
            
            // Get programs near the specified location
            var result = await _programService.GetNearbyProgramsAsync(latitude, longitude, radiusKm, page, pageSize);
            
            if (!result.Success)
            {
                _logger.LogWarning("Get nearby programs failed - {Error}", result.Errors);
                return BadRequest(result.Errors);
            }
            
            return Ok(result.Data);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchPrograms(
            [FromQuery] string query,
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("User identity not found when searching loyalty programs");
                return Unauthorized();
            }

            _logger.LogInformation("Customer user {UserId} searching programs with query: {Query}", 
                userIdClaim, query);
            
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query cannot be empty");
            }
            
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Invalid pagination parameters");
            }
            
            // TODO: Implement a proper search method in the service that filters by query
            // For now, we're just using GetAllProgramsAsync as a fallback
            var result = await _programService.GetAllProgramsAsync(null, page, pageSize);
            
            if (!result.Success)
            {
                _logger.LogWarning("Search programs failed - {Error}", result.Errors);
                return BadRequest(result.Errors);
            }
            
            // Filter results client-side for now based on the search query
            // This is not efficient and should be replaced with a proper search implementation
            if (!string.IsNullOrEmpty(query))
            {
                var filteredData = result.Data;
                filteredData.Items = filteredData.Items
                    .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                
                return Ok(filteredData);
            }
            
            return Ok(result.Data);
        }
    }
} 