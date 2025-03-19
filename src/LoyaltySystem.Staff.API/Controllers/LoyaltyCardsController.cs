using System;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Shared.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoyaltySystem.Staff.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Staff,Admin")]
    public class LoyaltyCardsController : BaseLoyaltyCardsController
    {
        private readonly new ILogger<LoyaltyCardsController> _logger;
        private readonly new ILoyaltyCardService _loyaltyCardService;
        private readonly IStaffAuthorizationService _staffAuthService;

        public LoyaltyCardsController(
            ILogger<LoyaltyCardsController> logger,
            ILoyaltyCardService loyaltyCardService,
            IStaffAuthorizationService staffAuthService)
            : base(logger, loyaltyCardService)
        {
            _logger = logger;
            _loyaltyCardService = loyaltyCardService;
            _staffAuthService = staffAuthService;
        }

        [HttpGet("qr/{qrCode}")]
        public async Task<IActionResult> GetByQrCode(string qrCode)
        {
            _logger.LogInformation("Staff retrieving loyalty card by QR code");
            
            var result = await _loyaltyCardService.GetByQrCodeAsync(qrCode);
            
            if (!result.Success)
            {
                _logger.LogWarning("Failed to retrieve loyalty card by QR code - {Error}", result.Errors);
                return NotFound(result.Errors);
            }
            
            return Ok(result.Data);
        }

        [HttpPost("issue-stamps")]
        public async Task<IActionResult> IssueStamps([FromBody] IssueStampsRequest request)
        {
            _logger.LogInformation("Staff issuing stamps to loyalty card {CardId}", request.CardId);
            
            // Verify staff is authorized for this store
            var storeId = new StoreId(request.StoreId);
            if (!await _staffAuthService.IsAuthorizedForStore(User, storeId))
            {
                _logger.LogWarning("Staff not authorized for store {StoreId}", storeId);
                return Forbid();
            }
            
            var result = await _loyaltyCardService.IssueStampsAsync(
                request.CardId,
                request.StampCount,
                storeId,
                request.PurchaseAmount,
                request.TransactionReference);
                
            if (!result.Success)
            {
                _logger.LogWarning("Failed to issue stamps - {Error}", result.Errors);
                return BadRequest(result.Errors);
            }
            
            return Ok(result.Data);
        }

        [HttpPost("add-points")]
        public async Task<IActionResult> AddPoints([FromBody] AddPointsRequest request)
        {
            _logger.LogInformation("Staff adding points to loyalty card {CardId}", request.CardId);
            
            // Verify staff is authorized for this store
            var storeId = new StoreId(request.StoreId);
            if (!await _staffAuthService.IsAuthorizedForStore(User, storeId))
            {
                _logger.LogWarning("Staff not authorized for store {StoreId}", storeId);
                return Forbid();
            }
            
            var result = await _loyaltyCardService.AddPointsAsync(
                request.CardId,
                request.Points,
                request.TransactionAmount,
                storeId,
                request.StaffId,
                request.PosTransactionId);
                
            if (!result.Success)
            {
                _logger.LogWarning("Failed to add points - {Error}", result.Errors);
                return BadRequest(result.Errors);
            }
            
            return Ok(result.Data);
        }

        [HttpPost("redeem-reward")]
        public async Task<IActionResult> RedeemReward([FromBody] RedeemRewardRequest request)
        {
            _logger.LogInformation("Staff redeeming reward {RewardId} for card {CardId}", request.RewardId, request.CardId);
            
            // Verify staff is authorized for this store
            var storeId = new StoreId(request.StoreId);
            if (!await _staffAuthService.IsAuthorizedForStore(User, storeId))
            {
                _logger.LogWarning("Staff not authorized for store {StoreId}", storeId);
                return Forbid();
            }
            
            var result = await _loyaltyCardService.RedeemRewardAsync(
                request.CardId,
                request.RewardId,
                storeId,
                request.StaffId,
                request.RedemptionData);
                
            if (!result.Success)
            {
                _logger.LogWarning("Failed to redeem reward - {Error}", result.Errors);
                return BadRequest(result.Errors);
            }
            
            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCard([FromBody] CreateLoyaltyCardDto request)
        {
            _logger.LogInformation("Staff creating loyalty card for customer {CustomerId}", request.CustomerId);
            
            var result = await _loyaltyCardService.CreateCardAsync(request);
            
            if (!result.Success)
            {
                _logger.LogWarning("Staff create loyalty card failed - {Error}", result.Errors);
                return BadRequest(result.Errors);
            }
            
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
        }
    }

    public class IssueStampsRequest
    {
        public LoyaltyCardId? CardId { get; set; }
        public int StampCount { get; set; }
        public Guid StoreId { get; set; }
        public decimal PurchaseAmount { get; set; }
        public string TransactionReference { get; set; } = string.Empty;
    }

    public class AddPointsRequest
    {
        public LoyaltyCardId? CardId { get; set; }
        public int Points { get; set; }
        public Guid StoreId { get; set; }
        public decimal TransactionAmount { get; set; }
        public Guid? StaffId { get; set; }
        public string PosTransactionId { get; set; } = string.Empty;
        public string TransactionReference { get; set; } = string.Empty;
    }

    public class RedeemRewardRequest
    {
        public LoyaltyCardId? CardId { get; set; }
        public RewardId? RewardId { get; set; }
        public Guid StoreId { get; set; }
        public Guid? StaffId { get; set; }
        public LoyaltySystem.Application.DTOs.RedeemRequestData RedemptionData { get; set; } = new LoyaltySystem.Application.DTOs.RedeemRequestData();
    }
} 