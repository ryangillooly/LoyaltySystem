using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.Admin.API.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILoyaltyCardService _cardService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(
        ICustomerService customerService,
        ILoyaltyCardService cardService,
        ILogger<CustomersController> logger)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCustomers([FromQuery] int skip = 0, [FromQuery] int limit = 50)
    {
        _logger.LogInformation("Admin requesting all customers (skip: {Skip}, limit: {Limit})", 
            skip, limit);
            
        if (limit < 1 || skip < 0)
            return BadRequest("Invalid pagination parameters");
            
        var result = await _customerService.GetAllAsync(skip, limit);
            
        if (!result.Success)
        {
            _logger.LogWarning("Get all customers failed - {Error}", result.Errors);
            return BadRequest(result.Errors);
        }
            
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] CustomerId id)
    {
        _logger.LogInformation("Admin requesting customer by ID: {CustomerId}", id);
            
        var result = await _customerService.GetCustomerByIdAsync(id.ToString());
            
        if (!result.Success)
        {
            _logger.LogWarning("Get customer failed for ID: {CustomerId} - {Error}", id, result.Errors);
            return NotFound(result.Errors);
        }
            
        return Ok(result.Data);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchCustomers([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("Admin searching customers with query: {Query} (page {Page}, size {PageSize})", 
            query, page, pageSize);
            
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest((object)"Search query is required");
        }
            
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest((object)"Invalid pagination parameters");
        }
            
        var result = await _customerService.SearchCustomersAsync(query, page, pageSize);
            
        if (!result.Success)
        {
            _logger.LogWarning("Search customers failed - {Error}", result.Errors);
            return BadRequest((object)result.Errors);
        }
            
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto request)
    {
        _logger.LogInformation("Admin creating customer with email: {Email}", request.Email);
            
        var result = await _customerService.CreateCustomerAsync(request);
        if (!result.Success)
        {
            _logger.LogWarning("Admin create customer failed - {Error}", result.Errors);
            return BadRequest(result.Errors);
        }
            
        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id.ToString() }, result.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer([FromRoute] CustomerId id, [FromBody] UpdateCustomerDto customer)
    {
        _logger.LogInformation("Admin updating customer ID: {CustomerId}", id);
            
        var result = await _customerService.UpdateCustomerAsync(id.ToString(), customer);
            
        if (!result.Success)
        {
            _logger.LogWarning("Admin update customer failed - {Error}", result.Errors);
            return BadRequest(result.Errors);
        }
            
        return Ok(result.Data);
    }

    [HttpGet("{id}/cards")]
    public async Task<IActionResult> GetCustomerCards(CustomerId id)
    {
        _logger.LogInformation("Admin requesting loyalty cards for customer ID: {CustomerId}", id);
            
        var customerResult = await _customerService.GetCustomerByIdAsync(id.ToString());
        if (!customerResult.Success)
        {
            _logger.LogWarning("Customer ID: {CustomerId} not found", id);
            return NotFound("Customer not found");
        }
            
        var result = await _cardService.GetByCustomerIdAsync(id);
            
        if (!result.Success)
        {
            _logger.LogWarning("Get customer loyalty cards failed for customer ID: {CustomerId} - {Error}", 
                id, result.Errors);
            return NotFound(result.Errors);
        }
            
        return Ok(result.Data);
    }

    [HttpPost("{id}/enroll")]
    public async Task<IActionResult> EnrollCustomerInProgram([FromRoute] CustomerId id, [FromQuery] string programId)
    {
        _logger.LogInformation("Admin enrolling customer ID: {CustomerId} in program ID: {ProgramId}", 
            id, programId);
            
        // Validate customer exists
        var customerResult = await _customerService.GetCustomerByIdAsync(id.ToString());
        if (!customerResult.Success)
        {
            _logger.LogWarning("Customer ID: {CustomerId} not found", id);
            return NotFound("Customer not found");
        }
        
        // Check if program ID is provided
        if (string.IsNullOrEmpty(programId))
        {
            _logger.LogWarning("Program ID not provided for enrollment");
            return BadRequest("Program ID is required");
        }
        
        // Check if customer is already enrolled in this program
        var existingCardsResult = await _cardService.GetByCustomerIdAsync(id);
        if (existingCardsResult is { Success: true, Data: { } })
        {
            var cards = existingCardsResult.Data.ToList();
            if (cards.Any(c => c.ProgramId.ToString() == programId))
            {
                _logger.LogWarning("Customer {CustomerId} is already enrolled in program {ProgramId}", id, programId);
                return BadRequest($"Customer is already enrolled in this program");
            }
        }
        
        // Create loyalty card for the customer
        var createCardDto = new CreateLoyaltyCardDto
        {
            CustomerId = id.ToString(),
            ProgramId = programId,
            Status = CardStatus.Active
        };
        
        var result = await _cardService.CreateCardAsync(createCardDto);

        if (!result.Success)
        {
            _logger.LogWarning("Admin enroll customer failed - {Error}", result.Errors);
            return BadRequest(result.Errors);
        }

        _logger.LogInformation("Successfully enrolled customer {CustomerId} in program {ProgramId}", id, programId);
        return Ok(result.Data);
    }

    [HttpGet("analytics/signups")]
    public async Task<IActionResult> GetSignupAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        _logger.LogInformation("Admin requesting customer signup analytics");
            
        var start = startDate ?? DateTime.UtcNow.AddMonths(-3);
        var end = endDate ?? DateTime.UtcNow;
            
        var result = await _customerService.GetCustomerSignupsByDateRangeAsync(start, end);
            
        if (!result.Success)
        {
            _logger.LogWarning("Get customer signup analytics failed - {Error}", result.Errors);
            return BadRequest((object)result.Errors);
        }
            
        return Ok(result.Data);
    }

    [HttpGet("analytics/demographics")]
    public async Task<IActionResult> GetCustomerDemographics()
    {
        _logger.LogInformation("Admin requesting customer demographics");
            
        var ageGroups = await _customerService.GetCustomerAgeGroupsAsync();
        var genderDistribution = await _customerService.GetCustomerGenderDistributionAsync();
        var topLocations = await _customerService.GetTopCustomerLocationsAsync(10);
            
        return Ok(new
        {
            AgeGroups = ageGroups.Values,
            GenderDistribution = genderDistribution.Values,
            TopLocations = topLocations
        });
    }

    [HttpGet("analytics/loyalty")]
    public async Task<IActionResult> GetLoyaltyAnalytics()
    {
        _logger.LogInformation("Admin requesting customer loyalty analytics");
            
        var cardCountsResult = await _cardService.GetCardCountByStatusAsync(CardStatus.Active);
        int activeCards = 0;

        var cardCount = cardCountsResult.Data as int?;
        if (cardCountsResult.Success && cardCount > 0)
        {
            activeCards = cardCount.Value;
        }
            
        var totalCustomers = await _customerService.GetTotalCustomerCountAsync();
        var customersWithCards = await _customerService.GetCustomersWithCardsCountAsync();
            
        // Calculate average cards per customer
        double averageCardsPerCustomer = 0;
        if (customersWithCards > 0)
        {
            averageCardsPerCustomer = (double)activeCards / customersWithCards;
        }
            
        // Calculate enrollment rate
        double enrollmentRate = 0;
        if (totalCustomers > 0)
        {
            enrollmentRate = (double) customersWithCards / totalCustomers;
        }
            
        return Ok(new
        {
            TotalCustomers = totalCustomers,
            CustomersWithCards = customersWithCards > 0 ? customersWithCards : 0,
            AverageCardsPerCustomer = averageCardsPerCustomer,
            ActiveCards = activeCards,
            EnrollmentRate = enrollmentRate
        });
    }
}