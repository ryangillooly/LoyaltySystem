using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.Admin.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;
        private readonly IStoreService _storeService;
        private readonly ILoyaltyProgramService _programService;
        private readonly ILogger<BusinessController> _logger;

        public BusinessController(
            IBusinessService businessService,
            IStoreService storeService,
            ILoyaltyProgramService programService,
            ILogger<BusinessController> logger)
        {
            _businessService = businessService ?? throw new ArgumentNullException(nameof(businessService));
            _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
            _programService = programService ?? throw new ArgumentNullException(nameof(programService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all businesses with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllBusinesses([FromQuery] int skip = 0, [FromQuery] int limit = 50)
        {
            _logger.LogInformation("Admin requesting all businesses (skip {skip}, limit {limit})", 
                skip, limit);
            
            if (limit < 1 || skip < 0)
                return BadRequest("Invalid pagination parameters");
            
            var result = await _businessService.GetAllBusinessesAsync(skip, limit);
            
            if (!result.Success)
            {
                _logger.LogWarning("Get all businesses failed - {Error}", result.Errors);
                return BadRequest(result.Errors);
            }
            
            return Ok(result.Data);
        }

        /// <summary>
        /// Get a specific business by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBusinessById([FromRoute] BusinessId id)
        {
            _logger.LogInformation("Admin requesting business by ID: {BusinessId}", id);
            
            var result = await _businessService.GetBusinessByIdAsync(id.ToString());
            
            if (!result.Success)
            {
                _logger.LogWarning("Get business failed for ID: {BusinessId} - {Error}", id, result.Errors);
                return NotFound(result.Errors);
            }
            
            return Ok(result.Data);
        }

        /// <summary>
        /// Get detailed business information including all brands
        /// </summary>
        [HttpGet("{id}/detail")]
        public async Task<IActionResult> GetBusinessDetail([FromRoute] BusinessId id)
        {
            _logger.LogInformation("Admin requesting detailed business information for ID: {BusinessId}", id);
            
            var result = await _businessService.GetBusinessDetailAsync(id.ToString());
            
            if (!result.Success)
            {
                _logger.LogWarning("Get business detail failed for ID: {BusinessId} - {Error}", id, result.Errors);
                return NotFound(result.Errors);
            }
            
            return Ok(result.Data);
        }

        /// <summary>
        /// Search businesses by name
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchBusinesses(
            [FromQuery] string nameQuery, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            _logger.LogInformation("Admin searching businesses with query: {NameQuery}", nameQuery);
            
            if (string.IsNullOrWhiteSpace(nameQuery))
            {
                return BadRequest("Search query cannot be empty");
            }
            
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Invalid pagination parameters");
            }
            
            var result = await _businessService.SearchBusinessesByNameAsync(nameQuery, page, pageSize);
            
            if (!result.Success)
            {
                _logger.LogWarning("Search businesses failed - {Error}", result.Errors);
                return BadRequest(result.Errors);
            }
            
            return Ok(result.Data);
        }

        /// <summary>
        /// Create a new business
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateBusiness([FromBody] CreateBusinessDto request)
        {
            _logger.LogInformation("Admin creating business with name: {Name}", request.Name);
            
            var result = await _businessService.CreateBusinessAsync(request);
            
            if (!result.Success)
            {
                _logger.LogWarning("Admin create business failed - {Error}", result.Errors);
                return BadRequest(result.Errors);
            }
            
            return CreatedAtAction(nameof(GetBusinessById), new { id = result.Data.Id }, result.Data);
        }

        /// <summary>
        /// Update an existing business
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBusiness([FromRoute] BusinessId id, [FromBody] UpdateBusinessDto request)
        {
            _logger.LogInformation("Admin updating business ID: {BusinessId}", id);
            
            var result = await _businessService.UpdateBusinessAsync(id.ToString(), request);
            
            if (!result.Success)
            {
                _logger.LogWarning("Admin update business failed - {Error}", result.Errors);
                return BadRequest(result.Errors);
            }
            
            return Ok(result.Data);
        }

        /// <summary>
        /// Delete a business
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBusiness([FromRoute] BusinessId id)
        {
            _logger.LogInformation("Admin deleting business ID: {BusinessId}", id);
            
            var result = await _businessService.DeleteBusinessAsync(id.ToString());
            
            if (!result.Success)
            {
                _logger.LogWarning("Admin delete business failed - {Error}", result.Errors);
                
                // If the error is related to existing brands, return a conflict error
                if (result.Errors.Contains("has associated brands"))
                {
                    return Conflict(result.Errors);
                }
                
                return BadRequest(result.Errors);
            }
            
            return Ok(new { Message = "Business deleted successfully" });
        }

        /// <summary>
        /// Get business overview for a specific business
        /// </summary>
        [HttpGet("{id}/overview")]
        public async Task<IActionResult> GetBusinessOverview([FromRoute] BusinessId id)
        {
            _logger.LogInformation("Admin requesting business overview for business ID: {BusinessId}", id);
            
            // First verify business exists
            var businessResult = await _businessService.GetBusinessByIdAsync(id.ToString());
            if (!businessResult.Success)
            {
                _logger.LogWarning("Business not found ID: {BusinessId}", id);
                return NotFound($"Business with ID {id} not found");
            }
            
            var business = businessResult.Data;
            
            // Get detailed business information with brands
            var detailResult = await _businessService.GetBusinessDetailAsync(id.ToString());
            if (!detailResult.Success)
            {
                _logger.LogWarning("Failed to get business details - {Error}", detailResult.Errors);
                return BadRequest(detailResult.Errors);
            }
            
            var businessDetail = detailResult.Data;
            
            // Count stores across all brands
            int totalStores = 0;
            int totalPrograms = 0;
            
            foreach (var brand in businessDetail.Brands)
            {
                // Get stores for each brand
                var storesResult = await _storeService.GetStoresByBrandIdAsync(brand.Id);
                if (storesResult.Success)
                {
                    totalStores += storesResult.Data.Count;
                }
                
                // Get programs for each brand
                var programsResult = await _programService.GetProgramsByBrandIdAsync(brand.Id);
                if (programsResult.Success)
                {
                    totalPrograms += programsResult.Data.Count;
                }
            }
            
            // Create business overview response
            var overview = new
            {
                Business = business,
                BrandCount = businessDetail.Brands.Count,
                Brands = businessDetail.Brands,
                StoreCount = totalStores,
                ProgramCount = totalPrograms
            };
            
            return Ok(overview);
        }

        /// <summary>
        /// Get business performance metrics
        /// </summary>
        [HttpGet("{id}/performance")]
        public async Task<IActionResult> GetBusinessPerformance(
            [FromRoute] BusinessId id,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            _logger.LogInformation("Admin requesting business performance for business ID: {BusinessId}", id);
            
            // Set default date range if not provided
            startDate ??= DateTime.UtcNow.AddMonths(-1);
            endDate ??= DateTime.UtcNow;
            
            // First verify business exists
            var businessResult = await _businessService.GetBusinessByIdAsync(id.ToString());
            if (!businessResult.Success)
            {
                _logger.LogWarning("Business not found ID: {BusinessId}", id);
                return NotFound($"Business with ID {id} not found");
            }
            
            // This would typically call methods to get performance metrics
            // For now, return a placeholder response
            var performance = new
            {
                Business = businessResult.Data.Name,
                Period = new { Start = startDate, End = endDate },
                Metrics = new
                {
                    TotalBrands = 0,
                    TotalStores = 0,
                    TotalCustomers = 0,
                    TotalTransactions = 0,
                    TotalRevenue = 0.0m,
                    AverageTransactionValue = 0.0m,
                    CustomerEngagement = 0.0,
                    LoyaltyProgramUtilization = 0.0
                },
                BrandPerformance = new List<object>()
            };
            
            return Ok(performance);
        }
    }
} 