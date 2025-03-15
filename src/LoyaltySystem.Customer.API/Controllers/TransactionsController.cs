using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
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
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILoyaltyCardService _loyaltyCardService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(
            ITransactionService transactionService,
            ILoyaltyCardService loyaltyCardService,
            ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _loyaltyCardService = loyaltyCardService ?? throw new ArgumentNullException(nameof(loyaltyCardService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Record a new transaction for the authenticated customer
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RecordTransaction([FromBody] RecordTransactionDto transactionDto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("User identity not found when recording transaction");
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
            var verifyOwnership = await _loyaltyCardService.VerifyCardOwnership(transactionDto.LoyaltyCardId, customerId);
            if (!verifyOwnership.Success)
            {
                _logger.LogWarning("User {UserId} attempted to record transaction for card {CardId} that doesn't belong to them",
                    userIdClaim, transactionDto.LoyaltyCardId);
                return BadRequest("Invalid loyalty card ID or card doesn't belong to you.");
            }

            // Record the transaction
            var result = await _transactionService.RecordCustomerTransaction(transactionDto);
            if (!result.Success)
            {
                _logger.LogError("Failed to record transaction for user {UserId}: {Error}", 
                    userIdClaim, result.Errors);
                return BadRequest(new { message = result.Errors });
            }

            _logger.LogInformation("Successfully recorded transaction {TransactionId} for user {UserId}",
                result.Data.Id, userIdClaim);
            
            return Created($"/api/transactions/{result.Data.Id}", result.Data);
        }
    }
} 