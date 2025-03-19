using System.Security.Claims;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Shared.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.Customer.API.Controllers;

[ApiController]
[Route("api/customer/[controller]")]
[Authorize]
public class LoyaltyCardsController : ControllerBase
{
    private readonly ILoyaltyProgramService _programService;
    private readonly ILoyaltyCardService _loyaltyCardService;
    private readonly ILogger _logger;

    public LoyaltyCardsController(
        ILoyaltyCardService loyaltyCardService,
        ILoyaltyProgramService programService,
        ILogger<LoyaltyCardsController> logger)
    {
        _programService = programService ?? throw new ArgumentNullException(nameof(programService));
        _loyaltyCardService = loyaltyCardService ?? throw new ArgumentNullException(nameof(loyaltyCardService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMyCards()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            _logger.LogWarning("User identity not found when requesting own loyalty cards");
            return Unauthorized();
        }
        
        var customerId = User.FindFirstValue("CustomerId");
        if (string.IsNullOrEmpty(customerId))
        {
            _logger.LogWarning("User {UserId} does not have a linked customer profile", userIdClaim);
            return BadRequest("You don't have a customer profile linked to your account.");
        }

        _logger.LogInformation("Customer {CustomerId} requesting their loyalty cards", customerId);
            
        var customerIdObj = new CustomerId(EntityId.Parse<CustomerId>(customerId));
        var result = await _loyaltyCardService.GetByCustomerIdAsync(customerIdObj);
            
        if (!result.Success)
        {
            _logger.LogWarning("Get customer loyalty cards failed for customer ID: {CustomerId} - {Error}", customerId, result.Errors);
            return NotFound(result.Errors);
        }
            
        return Ok(result.Data);
    }

    [HttpGet("{id}/transactions")]
    public async Task<IActionResult> GetCardTransactions(string id)
    {
        var customerId = User.FindFirstValue("CustomerId");
        if (string.IsNullOrEmpty(customerId))
        {
            _logger.LogWarning("User does not have a linked customer profile when requesting card transactions");
            return BadRequest("You don't have a customer profile linked to your account.");
        }

        _logger.LogInformation("Customer {CustomerId} requesting transactions for card {CardId}", customerId, id);
            
        // First verify this card belongs to the current customer
        var cardId = new LoyaltyCardId(EntityId.Parse<LoyaltyCardId>(id));
        var cardResult = await _loyaltyCardService.GetByIdAsync(cardId);
        if (!cardResult.Success)
        {
            _logger.LogWarning("Card {CardId} not found when customer {CustomerId} requested transactions", id, customerId);
            return NotFound("Card not found");
        }

        if (cardResult.Data.CustomerId.ToString() != customerId)
        {
            _logger.LogWarning("Customer {CustomerId} attempted to access transactions for card {CardId} belonging to {OwnerId}", customerId, id, cardResult.Data.CustomerId);
            return Forbid();
        }
            
        var result = await _loyaltyCardService.GetCardTransactionsAsync(cardId);
            
        if (!result.Success)
        {
            _logger.LogWarning("Get card transactions failed for card ID: {CardId} - {Error}", id, result.Errors);
            return BadRequest(result.Errors);
        }
            
        return Ok(result.Data);
    }

    [HttpPost("enroll")]
    public async Task<IActionResult> EnrollInProgram([FromBody] EnrollmentRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            _logger.LogWarning("User identity not found when enrolling in loyalty program");
            return Unauthorized();
        }

        var customerId = User.FindFirstValue("CustomerId");
        if (string.IsNullOrEmpty(customerId))
        {
            _logger.LogWarning("User {UserId} does not have a linked customer profile", userIdClaim);
            return BadRequest("You don't have a customer profile linked to your account.");
        }

        _logger.LogInformation("Customer {CustomerId} enrolling in program {ProgramId}", 
            customerId, request.ProgramId);
            
        // Verify the program exists and is active
        var programResult = await _programService.GetProgramByIdAsync(request.ProgramId?.ToString());
        if (!programResult.Success)
        {
            _logger.LogWarning("Program {ProgramId} not found when customer {CustomerId} attempted to enroll", 
                request.ProgramId, customerId);
            return NotFound("Program not found");
        }
            
        if (!programResult.Data.IsActive)
        {
            _logger.LogWarning("Customer {CustomerId} attempted to enroll in inactive program {ProgramId}", 
                customerId, request.ProgramId);
            return BadRequest("This program is not currently active");
        }
            
        // Create a new loyalty card for this customer and program
        var customerIdObj = new CustomerId(EntityId.Parse<CustomerId>(customerId));
        var programIdObj = request.ProgramId ?? throw new InvalidOperationException("Program ID cannot be null");

        // TODO: Not implemented fully. Create the LoyaltyCardDto
        var newCard = new CreateLoyaltyCardDto();
            
        var result = await _loyaltyCardService.CreateCardAsync(newCard);
            
        if (!result.Success)
        {
            _logger.LogWarning("Enrollment failed for customer {CustomerId} in program {ProgramId} - {Error}", 
                customerId, request.ProgramId, result.Errors);
            return BadRequest(result.Errors);
        }
            
        return CreatedAtAction(nameof(GetCardTransactions), new { id = result.Data.Id }, result.Data);
    }

    [HttpGet("{id}/qr-code")]
    public async Task<IActionResult> GetCardQrCode(string id)
    {
        var customerId = User.FindFirstValue("CustomerId");
        if (string.IsNullOrEmpty(customerId))
        {
            _logger.LogWarning("User does not have a linked customer profile when requesting card QR code");
            return BadRequest("You don't have a customer profile linked to your account.");
        }

        _logger.LogInformation("Customer {CustomerId} requesting QR code for card {CardId}", customerId, id);
            
        // First verify this card belongs to the current customer
        var cardId = new LoyaltyCardId(EntityId.Parse<LoyaltyCardId>(id));
        var cardResult = await _loyaltyCardService.GetByIdAsync(cardId);
        if (!cardResult.Success)
        {
            _logger.LogWarning("Card {CardId} not found when customer {CustomerId} requested QR code", 
                id, customerId);
            return NotFound("Card not found");
        }

        if (cardResult.Data.CustomerId.ToString() != customerId)
        {
            _logger.LogWarning("Customer {CustomerId} attempted to access QR code for card {CardId} belonging to {OwnerId}",
                customerId, id, cardResult.Data.CustomerId);
            return Forbid();
        }
            
        // Get or generate QR code
        var qrResult = await _loyaltyCardService.GetOrGenerateQrCodeAsync(cardId);
        if (!qrResult.Success)
        {
            _logger.LogWarning("Failed to get QR code for card {CardId}: {Error}", id, qrResult.Errors);
            return BadRequest(qrResult.Errors);
        }
            
        // Return the QR code data
        return Ok(new { QrCode = qrResult.Data });
    }

    [HttpGet("nearby-stores")]
    public async Task<IActionResult> GetNearbyStores([FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] double radiusKm = 10)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            _logger.LogWarning("User identity not found when requesting nearby stores");
            return Unauthorized();
        }

        _logger.LogInformation("User {UserId} requesting stores near location ({Latitude}, {Longitude})", 
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
            
        // This would typically call a store service to find nearby stores
        // For now, return a placeholder response
        return Ok(new[] 
        {
            new { Id = "store_1", Name = "Store 1", Distance = 0.5 },
            new { Id = "store_2", Name = "Store 2", Distance = 1.2 },
            new { Id = "store_3", Name = "Store 3", Distance = 2.7 }
        });
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCardById(LoyaltyCardId id)
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
            if (string.IsNullOrEmpty(customerIdClaim) || 
                !result.Data.CustomerId.ToString().Equals(customerIdClaim, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Unauthorized access attempt to loyalty card ID: {CardId} by user with customer ID: {CustomerId}", id, customerIdClaim);
                return Forbid();
            }
        }
        
        return Ok(result.Data);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetCardsByCustomerId(CustomerId customerId)
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
    public async Task<IActionResult> GetCardsByProgramId(LoyaltyProgramId programId)
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

public class EnrollmentRequest
{
    public LoyaltyProgramId? ProgramId { get; set; }
}