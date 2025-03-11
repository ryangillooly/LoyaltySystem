using System;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Services;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Shared.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoyaltySystem.Admin.API.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LoyaltyCardsController : BaseLoyaltyCardsController
    {
        private readonly LoyaltyProgramService _programService;

        public LoyaltyCardsController(
            LoyaltyCardService loyaltyCardService,
            LoyaltyProgramService programService,
            ILogger<LoyaltyCardsController> logger) 
            : base(logger, loyaltyCardService)
        {
            _programService = programService ?? throw new ArgumentNullException(nameof(programService));
        }

        // Admin can access any card without ownership checks
        public override async Task<IActionResult> GetById(LoyaltyCardId id)
        {
            _logger.LogInformation("Admin requesting loyalty card by ID: {CardId}", id);
            
            var result = await _loyaltyCardService.GetByIdAsync(id);
            
            if (!result.Success)
            {
                _logger.LogWarning("Admin get loyalty card failed for ID: {CardId} - {Error}", id, result.Errors);
                return NotFound(result.Errors);
            }
            
            return Ok(result.Data);
        }

        // Admin can access any customer's cards without ownership checks
        public override async Task<IActionResult> GetByCustomerId(CustomerId customerId)
        {
            _logger.LogInformation("Admin requesting loyalty cards for customer ID: {CustomerId}", customerId);
            
            var result = await _loyaltyCardService.GetByCustomerIdAsync(customerId);
            
            if (!result.Success)
            {
                _logger.LogWarning("Admin get customer loyalty cards failed for customer ID: {CustomerId} - {Error}", 
                    customerId, result.Errors);
                return NotFound(result.Errors);
            }
            
            return Ok(result.Data);
        }

        [HttpGet("program/{programId}")]
        public override async Task<IActionResult> GetByProgramId(LoyaltyProgramId programId)
        {
            _logger.LogInformation("Admin requesting loyalty cards for program ID: {ProgramId}", programId);
            
            var result = await _loyaltyCardService.GetByProgramIdAsync(programId);
            
            if (!result.Success)
            {
                _logger.LogWarning("Admin get program loyalty cards failed for program ID: {ProgramId} - {Error}", 
                    programId, result.Errors);
                return NotFound(result.Errors);
            }
            
            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCard([FromBody] CreateCardRequest request)
        {
            _logger.LogInformation("Admin creating loyalty card for customer ID: {CustomerId} in program ID: {ProgramId}", 
                request.CustomerId, request.ProgramId);
            
            var result = await _loyaltyCardService.CreateCardAsync(request.CustomerId, request.ProgramId);
            
            if (!result.Success)
            {
                _logger.LogWarning("Admin create loyalty card failed - {Error}", result.Errors);
                return BadRequest(result.Errors);
            }
            
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateCardStatus(LoyaltyCardId id, [FromBody] UpdateCardStatusRequest request)
        {
            _logger.LogInformation("Admin updating loyalty card status for ID: {CardId} to {Status}", 
                id, request.Status);
            
            var result = await _loyaltyCardService.UpdateCardStatusAsync(id, Enum.Parse<CardStatus>(request.Status));
            
            if (!result.Success)
            {
                _logger.LogWarning("Admin update loyalty card status failed for ID: {CardId} - {Error}", 
                    id, result.Errors);
                return BadRequest(result.Errors);
            }
            
            return Ok(result.Data);
        }

        [HttpGet("analytics/status")]
        public async Task<IActionResult> GetCardCountByStatus()
        {
            _logger.LogInformation("Admin requesting loyalty card count by status");
            
            var active = await _loyaltyCardService.GetCardCountByStatusAsync(CardStatus.Active);
            var suspended = await _loyaltyCardService.GetCardCountByStatusAsync(CardStatus.Suspended);
            var expired = await _loyaltyCardService.GetCardCountByStatusAsync(CardStatus.Expired);
            
            return Ok(new 
            {
                Active = active.Data,
                Suspended = suspended.Data,
                Expired = expired.Data
            });
        }

        [HttpGet("analytics/program/{programId}")]
        public async Task<IActionResult> GetProgramCardAnalytics(LoyaltyProgramId programId)
        {
            _logger.LogInformation("Admin requesting card analytics for program ID: {ProgramId}", programId);
            
            // Get program details first to ensure it exists
            var programResult = await _programService.GetProgramByIdAsync(programId.ToString());
            if (!programResult.Success)
            {
                return NotFound($"Program with ID {programId} not found");
            }
            
            var activeCount = await _loyaltyCardService.GetActiveCardCountForProgramAsync(programId.ToString());
            var totalCards = await _loyaltyCardService.GetCardCountForProgramAsync(programId.ToString());
            var avgTransactions = await _loyaltyCardService.GetAverageTransactionsPerCardForProgramAsync(programId.ToString());
            
            return Ok(new
            {
                ProgramName = programResult.Data?.Name,
                TotalCards = totalCards,
                ActiveCards = activeCount,
                AverageTransactionsPerCard = avgTransactions
            });
        }
    }

    public class UpdateCardStatusRequest
    {
        public string Status { get; set; }
    }
} 