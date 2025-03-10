using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Services;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoyaltyCardsController : ControllerBase
    {
        private readonly LoyaltyCardService _loyaltyCardService;

        public LoyaltyCardsController(LoyaltyCardService loyaltyCardService)
        {
            _loyaltyCardService = loyaltyCardService ?? throw new ArgumentNullException(nameof(loyaltyCardService));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(LoyaltyCardId id)
        {
            var result = await _loyaltyCardService.GetCardByIdAsync(id);
            
            if (!result.Success)
                return NotFound(result.Errors);
            
            // Verify ownership or staff role if not admin
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                var customerIdClaim = User.FindFirstValue("CustomerId");
                if (string.IsNullOrEmpty(customerIdClaim) || 
                    !result.Data.CustomerId.ToString().Equals(customerIdClaim, StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }
            }
                
            return Ok(result.Data);
        }

        [HttpGet("qr/{qrCode}")]
        [Authorize(Roles = "Admin,StoreManager,Staff")]
        public async Task<IActionResult> GetByQrCode(string qrCode)
        {
            var result = await _loyaltyCardService.GetCardByQrCodeAsync(qrCode);
            
            if (!result.Success)
                return NotFound(result.Errors);
                
            return Ok(result.Data);
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomerId(CustomerId customerId)
        {
            // Verify ownership or staff role if not admin
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                var userCustomerId = User.FindFirstValue("CustomerId");
                if (string.IsNullOrEmpty(userCustomerId) || 
                    !customerId.ToString().Equals(userCustomerId, StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }
            }
            
            var result = await _loyaltyCardService.GetCardsByCustomerIdAsync(customerId);
            
            if (!result.Success)
                return NotFound(result.Errors);
                
            return Ok(result.Data);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,StoreManager,Staff")]
        public async Task<IActionResult> Create(CreateCardRequest request)
        {
            var result = await _loyaltyCardService.CreateCardAsync(request.CustomerId, request.ProgramId);
            
            if (!result.Success)
                return BadRequest(result.Errors);
                
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
        }

        [HttpPost("stamps")]
        [Authorize(Roles = "Admin,StoreManager,Staff")]
        public async Task<IActionResult> IssueStamps(IssueStampsRequest request)
        {
            var result = await _loyaltyCardService.IssueStampsAsync(request);
            
            if (!result.Success)
                return BadRequest(result.Errors);
                
            return Ok(result.Data);
        }

        [HttpPost("points")]
        [Authorize(Roles = "Admin,StoreManager,Staff")]
        public async Task<IActionResult> AddPoints(AddPointsRequest request)
        {
            var result = await _loyaltyCardService.AddPointsAsync(request);
            
            if (!result.Success)
                return BadRequest(result.Errors);
                
            return Ok(result.Data);
        }

        [HttpPost("redeem")]
        [Authorize(Roles = "Admin,StoreManager,Staff")]
        public async Task<IActionResult> RedeemReward(RedeemRewardRequest request)
        {
            var result = await _loyaltyCardService.RedeemRewardAsync(request);
            
            if (!result.Success)
                return BadRequest(result.Errors);
                
            return Ok(result.Data);
        }
    }

    /// <summary>
    /// Request model for creating a loyalty card.
    /// </summary>
    public class CreateCardRequest
    {
        /// <summary>
        /// The customer for whom to create the card.
        /// </summary>
        public CustomerId CustomerId { get; set; }
        
        /// <summary>
        /// The loyalty program to enroll in.
        /// </summary>
        public LoyaltyProgramId ProgramId { get; set; }
    }
} 