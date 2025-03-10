using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Services;

namespace LoyaltySystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly LoyaltyCardService _loyaltyCardService;

        public CustomersController(LoyaltyCardService loyaltyCardService)
        {
            _loyaltyCardService = loyaltyCardService ?? throw new ArgumentNullException(nameof(loyaltyCardService));
        }

        [HttpGet("{customerId}/cards")]
        public async Task<IActionResult> GetCustomerCards(Guid customerId)
        {
            var result = await _loyaltyCardService.GetCardsByCustomerIdAsync(customerId);
            
            if (!result.Success)
                return NotFound(result.Errors);
                
            return Ok(result.Data);
        }

        [HttpPost("{customerId}/programs/{programId}/enroll")]
        public async Task<IActionResult> EnrollInProgram(Guid customerId, Guid programId)
        {
            var result = await _loyaltyCardService.CreateCardAsync(customerId, programId);
            
            if (!result.Success)
                return BadRequest(result.Errors);
                
            return CreatedAtAction(
                nameof(LoyaltyCardsController.GetById), 
                "LoyaltyCards", 
                new { id = result.Data.Id }, 
                result.Data);
        }

        [HttpPost("{customerId}/cards/{cardId}/stamps")]
        public async Task<IActionResult> IssueStamps(Guid customerId, Guid cardId, IssueStampsRequest request)
        {
            // Validate that the card belongs to the customer
            if (request.CardId != cardId)
            {
                request.CardId = cardId;
            }
            
            var result = await _loyaltyCardService.IssueStampsAsync(request);
            
            if (!result.Success)
                return BadRequest(result.Errors);
                
            return Ok(result.Data);
        }

        [HttpPost("{customerId}/cards/{cardId}/points")]
        public async Task<IActionResult> AddPoints(Guid customerId, Guid cardId, AddPointsRequest request)
        {
            // Validate that the card belongs to the customer
            if (request.CardId != cardId)
            {
                request.CardId = cardId;
            }
            
            var result = await _loyaltyCardService.AddPointsAsync(request);
            
            if (!result.Success)
                return BadRequest(result.Errors);
                
            return Ok(result.Data);
        }

        [HttpPost("{customerId}/cards/{cardId}/rewards/{rewardId}/redeem")]
        public async Task<IActionResult> RedeemReward(Guid customerId, Guid cardId, Guid rewardId, [FromBody] RedeemRequestData requestData)
        {
            var request = new RedeemRewardRequest
            {
                CardId = cardId,
                RewardId = rewardId,
                StoreId = requestData.StoreId,
                StaffId = requestData.StaffId
            };
            
            var result = await _loyaltyCardService.RedeemRewardAsync(request);
            
            if (!result.Success)
                return BadRequest(result.Errors);
                
            return Ok(result.Data);
        }
    }

    /// <summary>
    /// Data for redeeming a reward.
    /// </summary>
    public class RedeemRequestData
    {
        /// <summary>
        /// The store where the reward is being redeemed.
        /// </summary>
        public Guid StoreId { get; set; }
        
        /// <summary>
        /// The staff member processing the redemption.
        /// </summary>
        public Guid? StaffId { get; set; }
    }
} 