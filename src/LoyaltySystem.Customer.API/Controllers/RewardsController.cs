using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.LoyaltyPrograms;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoyaltySystem.Customer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RewardsController : ControllerBase
    {
        private readonly ILoyaltyRewardsService _rewardsService;
        private readonly ILoyaltyCardService _loyaltyCardService;
        private readonly ILogger<RewardsController> _logger;

        public RewardsController(
            ILoyaltyRewardsService rewardsService,
            ILoyaltyCardService loyaltyCardService,
            ILogger<RewardsController> logger)
        {
            _rewardsService = rewardsService ?? throw new ArgumentNullException(nameof(rewardsService));
            _loyaltyCardService = loyaltyCardService ?? throw new ArgumentNullException(nameof(loyaltyCardService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Redeem a reward for the authenticated customer
        /// </summary>
        [HttpPost("redeem")]
        public async Task<IActionResult> RedeemReward([FromBody] RedeemRewardDto redeemDto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("User identity not found when attempting to redeem reward");
                return Unauthorized();
            }

            var customerIdClaim = User.FindFirstValue("CustomerId");
            if (string.IsNullOrEmpty(customerIdClaim))
            {
                _logger.LogWarning("User {UserId} does not have a linked customer profile", userIdClaim);
                return BadRequest("You don't have a customer profile linked to your account.");
            }

            var customerId = CustomerId.Parse<CustomerId>(customerIdClaim);
            
            // Verify the loyalty card belongs to the customer
            var verifyOwnership = await _loyaltyCardService.VerifyCardOwnership(redeemDto.LoyaltyCardId, customerId);
            if (!verifyOwnership.Success)
            {
                _logger.LogWarning("User {UserId} attempted to redeem reward with card {CardId} that doesn't belong to them",
                    userIdClaim, redeemDto.LoyaltyCardId);
                return BadRequest("Invalid loyalty card ID or card doesn't belong to you.");
            }

            // Check if customer has enough points/stamps for the reward
            var checkEligibility = await _rewardsService.CheckRewardEligibility(redeemDto.RewardId, redeemDto.LoyaltyCardId);
            if (!checkEligibility.Success)
            {
                _logger.LogWarning("User {UserId} is not eligible for reward {RewardId}: {Reason}",
                    userIdClaim, redeemDto.RewardId, checkEligibility.Errors);
                return BadRequest(new { message = checkEligibility.Errors });
            }

            // Process the redemption
            var result = await _rewardsService.RedeemReward(redeemDto);
            if (!result.Success)
            {
                _logger.LogError("Failed to redeem reward for user {UserId}: {Error}",
                    userIdClaim, result.Errors);
                return BadRequest(new { message = result.Errors });
            }

            _logger.LogInformation("Successfully redeemed reward {RewardId} for user {UserId}, redemption ID: {RedemptionId}",
                redeemDto.RewardId, userIdClaim, result.Data.Id);
            
            return Ok(result.Data);
        }
    }
} 