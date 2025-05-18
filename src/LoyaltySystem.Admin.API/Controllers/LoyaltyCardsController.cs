using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.Admin.API.Controllers;

[ApiController]
[Route("api/loyalty-cards")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class LoyaltyCardsController : ControllerBase
{
    private readonly ILoyaltyProgramService _programService;
    private readonly ILogger _logger;
    private readonly ILoyaltyCardService _loyaltyCardService;

    public LoyaltyCardsController(
        ILoyaltyCardService loyaltyCardService,
        ILoyaltyProgramService programService,
        ILogger<LoyaltyCardsController> logger)
    {
        _programService = programService ?? throw new ArgumentNullException(nameof(programService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _loyaltyCardService = loyaltyCardService ?? throw new ArgumentNullException(nameof(loyaltyCardService));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] int skip = 0, [FromQuery] int limit = 50)
    {
        _logger.LogInformation("Admin requesting all loyalty cards");
            
        var result = await _loyaltyCardService.GetAllAsync(skip, limit);
            
        if (!result.Success)
        {
            _logger.LogWarning("Admin get loyalty cards failed: {Error}", result.Errors);
            return NotFound(result.Errors);
        }
            
        return Ok(result.Data);
    }
        
    /// <summary>
    /// Retrieves a loyalty card by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the loyalty card.</param>
    /// <returns>HTTP 200 with the loyalty card data if found; otherwise, HTTP 404 with error details.</returns>
    [HttpGet("{id}")]
    // Admin can access any card without ownership checks
    public async Task<IActionResult> GetById(string id)
    {
        _logger.LogInformation("Admin requesting loyalty card by ID: {CardId}", id);
            
        var result = await _loyaltyCardService.GetByIdAsync(new LoyaltyCardId(EntityId.Parse<LoyaltyCardId>(id)));
            
        if (!result.Success)
        {
            _logger.LogWarning("Admin get loyalty card failed for ID: {CardId} - {Error}", id, result.Errors);
            return NotFound(result.Errors);
        }
            
        return Ok(result.Data);
    }

    /// <summary>
    /// Retrieves all loyalty cards associated with the specified customer ID.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer whose loyalty cards are to be retrieved.</param>
    /// <returns>An HTTP 200 response with the list of loyalty cards if found; otherwise, HTTP 404 with error details.</returns>
    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomerId(CustomerId customerId)
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
    public async Task<IActionResult> GetByProgramId(LoyaltyProgramId programId)
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
    public async Task<IActionResult> CreateCard([FromBody] CreateLoyaltyCardDto request)
    {
        _logger.LogInformation("Admin creating loyalty card for customer ID: {CustomerId} in program ID: {ProgramId}", 
            request.CustomerId, request.ProgramId);
            
        var result = await _loyaltyCardService.CreateCardAsync(request);
            
        if (!result.Success)
        {
            _logger.LogWarning("Admin create loyalty card failed - {Error}", result.Errors);
            return BadRequest(result.Errors);
        }
            
        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateCardStatus([FromRoute] LoyaltyCardId id, [FromBody] UpdateCardStatusRequest request)
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