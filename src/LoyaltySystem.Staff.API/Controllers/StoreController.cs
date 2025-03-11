using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoyaltySystem.Staff.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Staff,Admin")]
    public class StoreController : ControllerBase
    {
        private readonly StoreService _storeService;
        private readonly LoyaltyProgramService _programService;
        private readonly ILogger<StoreController> _logger;

        public StoreController(
            StoreService storeService,
            LoyaltyProgramService programService,
            ILogger<StoreController> logger)
        {
            _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
            _programService = programService ?? throw new ArgumentNullException(nameof(programService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentStore()
        {
            string storeIdClaim = User.FindFirstValue("StoreId");
            if (string.IsNullOrEmpty(storeIdClaim))
            {
                _logger.LogWarning("Staff user does not have a store assigned");
                return BadRequest("No store assigned to your account");
            }

            Guid storeId;
            if (!Guid.TryParse(storeIdClaim, out storeId))
            {
                _logger.LogWarning("Invalid store ID format in claim: {StoreId}", storeIdClaim);
                return BadRequest("Invalid store ID format");
            }

            _logger.LogInformation("Staff fetching current store details for store ID: {StoreId}", storeId);
            
            var result = await _storeService.GetStoreByIdAsync(storeId.ToString());
            
            if (!result.Success)
            {
                _logger.LogWarning("Get store failed for ID: {StoreId} - {Error}", storeId, result.Errors);
                return NotFound(result.Errors);
            }
            
            return Ok(result.Data);
        }

        [HttpGet("current/programs")]
        public async Task<IActionResult> GetStorePrograms()
        {
            string storeIdClaim = User.FindFirstValue("StoreId");
            if (string.IsNullOrEmpty(storeIdClaim))
            {
                _logger.LogWarning("Staff user does not have a store assigned");
                return BadRequest("No store assigned to your account");
            }

            Guid storeId;
            if (!Guid.TryParse(storeIdClaim, out storeId))
            {
                _logger.LogWarning("Invalid store ID format in claim: {StoreId}", storeIdClaim);
                return BadRequest("Invalid store ID format");
            }

            _logger.LogInformation("Staff fetching programs for store ID: {StoreId}", storeId);
            
            // First get the store to check the brand ID
            var storeResult = await _storeService.GetStoreByIdAsync(storeId.ToString());
            if (!storeResult.Success)
            {
                _logger.LogWarning("Get store failed for ID: {StoreId} - {Error}", storeId, storeResult.Errors);
                return NotFound("Store not found");
            }
            
            var result = await _programService.GetProgramsByBrandIdAsync(storeResult.Data.BrandId);
            
            if (!result.Success)
            {
                _logger.LogWarning("Get brand programs failed for brand ID: {BrandId} - {Error}", 
                    storeResult.Data.BrandId, result.Errors);
                return NotFound(result.Errors);
            }
            
            // Filter for active programs only
            var activePrograms = result.Data.FindAll(p => p.IsActive);
            
            return Ok(activePrograms);
        }

        [HttpGet("current/rewards")]
        public async Task<IActionResult> GetStoreRewards()
        {
            string storeIdClaim = User.FindFirstValue("StoreId");
            if (string.IsNullOrEmpty(storeIdClaim))
            {
                _logger.LogWarning("Staff user does not have a store assigned");
                return BadRequest("No store assigned to your account");
            }

            Guid storeId;
            if (!Guid.TryParse(storeIdClaim, out storeId))
            {
                _logger.LogWarning("Invalid store ID format in claim: {StoreId}", storeIdClaim);
                return BadRequest("Invalid store ID format");
            }

            _logger.LogInformation("Staff fetching rewards for store ID: {StoreId}", storeId);
            
            // First get the store to check the brand ID
            var storeResult = await _storeService.GetStoreByIdAsync(storeId.ToString());
            if (!storeResult.Success)
            {
                _logger.LogWarning("Get store failed for ID: {StoreId} - {Error}", storeId, storeResult.Errors);
                return NotFound("Store not found");
            }
            
            var programsResult = await _programService.GetProgramsByBrandIdAsync(storeResult.Data.BrandId);
            if (!programsResult.Success)
            {
                _logger.LogWarning("Get brand programs failed for brand ID: {BrandId} - {Error}", 
                    storeResult.Data.BrandId, programsResult.Errors);
                return NotFound(programsResult.Errors);
            }
            
            // Collect all active rewards from all active programs
            var rewards = new List<RewardDto>();
            foreach (var program in programsResult.Data.Where(p => p.IsActive))
            {
                var rewardsResult = await _programService.GetRewardsByProgramIdAsync(program.Id);
                if (rewardsResult.Success)
                {
                    // Only add active rewards
                    rewards.AddRange(rewardsResult.Data.Where(r => r.IsActive));
                }
            }
            
            return Ok(rewards);
        }

        [HttpGet("current/transactions")]
        public async Task<IActionResult> GetStoreTransactions(
            [FromQuery] DateTime? startDate, 
            [FromQuery] DateTime? endDate, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            string storeIdClaim = User.FindFirstValue("StoreId");
            if (string.IsNullOrEmpty(storeIdClaim))
            {
                _logger.LogWarning("Staff user does not have a store assigned");
                return BadRequest("No store assigned to your account");
            }

            Guid storeId;
            if (!Guid.TryParse(storeIdClaim, out storeId))
            {
                _logger.LogWarning("Invalid store ID format in claim: {StoreId}", storeIdClaim);
                return BadRequest("Invalid store ID format");
            }

            var start = startDate ?? DateTime.UtcNow.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow;

            _logger.LogInformation("Staff fetching transactions for store ID: {StoreId} from {StartDate} to {EndDate}", 
                storeId, start, end);
            
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Invalid pagination parameters");
            }
            
            var result = await _storeService.GetStoreTransactionsAsync(storeId.ToString(), start, end, page, pageSize);
            
            if (!result.Success)
            {
                _logger.LogWarning("Get store transactions failed for store ID: {StoreId} - {Error}", 
                    storeId, result.Errors);
                return BadRequest(result.Errors);
            }
            
            return Ok(result.Data);
        }

        [HttpGet("current/stats")]
        public async Task<IActionResult> GetStoreStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            string storeIdClaim = User.FindFirstValue("StoreId");
            if (string.IsNullOrEmpty(storeIdClaim))
            {
                _logger.LogWarning("Staff user does not have a store assigned");
                return BadRequest("No store assigned to your account");
            }

            Guid storeId;
            if (!Guid.TryParse(storeIdClaim, out storeId))
            {
                _logger.LogWarning("Invalid store ID format in claim: {StoreId}", storeIdClaim);
                return BadRequest("Invalid store ID format");
            }

            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            _logger.LogInformation("Staff fetching stats for store ID: {StoreId} from {StartDate} to {EndDate}", 
                storeId, start, end);
            
            var transactionCount = await _storeService.GetTransactionCountAsync(storeId.ToString(), start, end);
            var stampsIssued = await _storeService.GetTotalStampsIssuedAsync(storeId.ToString(), start, end);
            var pointsIssued = await _storeService.GetTotalPointsIssuedAsync(storeId.ToString(), start, end);
            var redemptionCount = await _storeService.GetRedemptionCountAsync(storeId.ToString(), start, end);
            
            return Ok(new
            {
                StoreId = storeId,
                PeriodStart = start,
                PeriodEnd = end,
                TransactionCount = transactionCount,
                StampsIssued = stampsIssued,
                PointsIssued = pointsIssued,
                RedemptionCount = redemptionCount
            });
        }

        [HttpGet("current/hours")]
        public async Task<IActionResult> GetStoreHours()
        {
            string storeIdClaim = User.FindFirstValue("StoreId");
            if (string.IsNullOrEmpty(storeIdClaim))
            {
                _logger.LogWarning("Staff user does not have a store assigned");
                return BadRequest("No store assigned to your account");
            }

            Guid storeId;
            if (!Guid.TryParse(storeIdClaim, out storeId))
            {
                _logger.LogWarning("Invalid store ID format in claim: {StoreId}", storeIdClaim);
                return BadRequest("Invalid store ID format");
            }

            _logger.LogInformation("Staff fetching operating hours for store ID: {StoreId}", storeId);
            
            var result = await _storeService.GetStoreOperatingHoursAsync(storeId.ToString());
            
            if (!result.Success)
            {
                _logger.LogWarning("Get store hours failed for store ID: {StoreId} - {Error}", 
                    storeId, result.Errors);
                return NotFound(result.Errors);
            }
            
            return Ok(result.Data);
        }
    }
} 