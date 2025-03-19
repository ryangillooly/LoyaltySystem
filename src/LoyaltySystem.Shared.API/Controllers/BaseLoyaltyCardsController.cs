using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoyaltySystem.Shared.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public abstract class BaseLoyaltyCardsController : ControllerBase
    {
        protected readonly ILogger _logger;
        protected readonly ILoyaltyCardService _loyaltyCardService;

        protected BaseLoyaltyCardsController(
            ILogger logger,
            ILoyaltyCardService loyaltyCardService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loyaltyCardService = loyaltyCardService ?? throw new ArgumentNullException(nameof(loyaltyCardService));
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetById(LoyaltyCardId id)
        {
            _logger.LogInformation("Retrieving loyalty card by ID: {CardId}", id);
            
            var result = await _loyaltyCardService.GetByIdAsync(id);
            
            if (!result.Success)
            {
                _logger.LogWarning("Get loyalty card failed for ID: {CardId} - {Error}", id, result.Errors);
                return NotFound(result.Errors);
            }
            
            // Verify ownership or staff role if not admin
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                var customerIdClaim = User.FindFirstValue("CustomerId");
                if (string.IsNullOrEmpty(customerIdClaim) || !result.Data.CustomerId.ToString().Equals(customerIdClaim, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Unauthorized access attempt to loyalty card ID: {CardId} by user with customer ID: {CustomerId}", id, customerIdClaim);
                    return Forbid();
                }
            }
            
            return Ok(result.Data);
        }

        [HttpGet("customer/{customerId}")]
        public virtual async Task<IActionResult> GetByCustomerId(CustomerId customerId)
        {
            _logger.LogInformation("Retrieving loyalty cards for customer ID: {CustomerId}", customerId);
            
            // Verify ownership or staff role if not admin
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                var userCustomerId = User.FindFirstValue("CustomerId");
                if (string.IsNullOrEmpty(userCustomerId) || 
                    !customerId.ToString().Equals(userCustomerId, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Unauthorized access attempt to customer ID: {CustomerId} by user with customer ID: {UserCustomerId}",
                        customerId, userCustomerId);
                    return Forbid();
                }
            }
            
            var result = await _loyaltyCardService.GetByCustomerIdAsync(customerId);
            
            if (!result.Success)
            {
                _logger.LogWarning("Get loyalty cards failed for customer ID: {CustomerId} - {Error}", 
                    customerId, result.Errors);
                return NotFound(result.Errors);
            }
            
            return Ok(result.Data);
        }

        [HttpGet("program/{programId}")]
        public virtual async Task<IActionResult> GetByProgramId(LoyaltyProgramId programId)
        {
            _logger.LogInformation("Retrieving loyalty cards for program ID: {ProgramId}", programId);
            
            var result = await _loyaltyCardService.GetByProgramIdAsync(programId);
            
            if (!result.Success)
            {
                _logger.LogWarning("Get loyalty cards failed for program ID: {ProgramId} - {Error}", 
                    programId, result.Errors);
                return NotFound(result.Errors);
            }
            
            return Ok(result.Data);
        }
    }
} 