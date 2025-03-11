using System;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Services;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoyaltySystem.Admin.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class CustomersController : ControllerBase
    {
        private readonly CustomerService _customerService;
        private readonly LoyaltyCardService _cardService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(
            CustomerService customerService,
            LoyaltyCardService cardService,
            ILogger<CustomersController> logger)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCustomers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            _logger.LogInformation("Admin requesting all customers (page {Page}, size {PageSize})", 
                page, pageSize);
            
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest((object)"Invalid pagination parameters");
            }
            
            var result = await _customerService.GetAllCustomersAsync(page, pageSize);
            
            if (!result.Success)
            {
                _logger.LogWarning("Get all customers failed - {Error}", result.Errors);
                return BadRequest((object)result.Errors);
            }
            
            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(CustomerId id)
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
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
        {
            _logger.LogInformation("Admin creating customer with email: {Email}", request.Email);
            
            var createCustomerDto = new LoyaltySystem.Application.Services.CreateCustomerDto
            {
                Name = $"{request.FirstName} {request.LastName}",
                Email = request.Email,
                Phone = request.PhoneNumber
            };
            
            var result = await _customerService.CreateCustomerAsync(createCustomerDto);
            
            if (!result.Success)
            {
                _logger.LogWarning("Admin create customer failed - {Error}", result.Errors);
                return BadRequest(result.Errors);
            }
            
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(CustomerId id, [FromBody] UpdateCustomerRequest request)
        {
            _logger.LogInformation("Admin updating customer ID: {CustomerId}", id);
            
            var updateCustomerDto = new LoyaltySystem.Application.Services.UpdateCustomerDto
            {
                Name = $"{request.FirstName} {request.LastName}",
                Email = request.Email,
                Phone = request.PhoneNumber
            };
            
            var result = await _customerService.UpdateCustomerAsync(id.ToString(), updateCustomerDto);
            
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
        public async Task<IActionResult> EnrollCustomerInProgram(CustomerId id, [FromBody] EnrollCustomerRequest request)
        {
            _logger.LogInformation("Admin enrolling customer ID: {CustomerId} in program ID: {ProgramId}", 
                id, request.ProgramId);
            
            var customerResult = await _customerService.GetCustomerByIdAsync(id.ToString());
            if (!customerResult.Success)
            {
                _logger.LogWarning("Customer ID: {CustomerId} not found", id);
                return NotFound("Customer not found");
            }
            
            var result = await _cardService.CreateCardAsync(id, request.ProgramId);
            
            if (!result.Success)
            {
                _logger.LogWarning("Admin enroll customer failed - {Error}", result.Errors);
                return BadRequest((object)result.Errors);
            }
            
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

    public class CreateCustomerRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public LoyaltySystem.Application.DTOs.AddressDto Address { get; set; }
    }

    public class UpdateCustomerRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public LoyaltySystem.Application.DTOs.AddressDto Address { get; set; }
        public bool IsActive { get; set; }
    }

    public class EnrollCustomerRequest
    {
        public LoyaltyProgramId ProgramId { get; set; }
    }
} 