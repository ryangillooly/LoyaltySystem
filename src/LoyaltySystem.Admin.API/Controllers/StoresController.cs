using System;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Services;
using LoyaltySystem.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoyaltySystem.Admin.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class StoresController : ControllerBase
    {
        private readonly StoreService _storeService;
        private readonly BrandService _brandService;
        private readonly ILogger<StoresController> _logger;

        public StoresController(
            StoreService storeService,
            BrandService brandService,
            ILogger<StoresController> logger)
        {
            _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
            _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all stores with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllStores([FromQuery] int skip = 0, [FromQuery] int limit = 50)
        {
            _logger.LogInformation("Admin requesting all stores (skip: {skip}, limit: {limit})", 
                skip, limit);
            
            if (limit < 1 || skip < 0)
                return BadRequest("Invalid pagination parameters");
            
            var result = await _storeService.GetAllStoresAsync(skip, limit);
            
            if (!result.Success)
            {
                _logger.LogWarning("Get all stores failed - {Error}", result.Errors);
                return BadRequest(result.Errors);
            }
            
            return Ok(result.Data);
        }

        /// <summary>
        /// Get a specific store by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStoreById([FromRoute] StoreId id)
        {
            _logger.LogInformation("Admin requesting store by ID: {StoreId}", id);
            
            var result = await _storeService.GetStoreByIdAsync(id.ToString());
            
            if (!result.Success)
            {
                _logger.LogWarning("Get store failed for ID: {StoreId} - {Error}", id, result.Errors);
                return NotFound(result.Errors);
            }
            
            return Ok(result.Data);
        }

        /// <summary>
        /// Get stores for a specific brand
        /// </summary>
        [HttpGet("brand/{brandId}")]
        public async Task<IActionResult> GetStoresByBrandId([FromRoute] BrandId brandId)
        {
            _logger.LogInformation("Admin requesting stores for brand ID: {BrandId}", brandId);
            
            // First verify brand exists
            var brandResult = await _brandService.GetBrandByIdAsync(brandId.ToString());
            if (!brandResult.Success)
            {
                _logger.LogWarning("Brand not found ID: {BrandId}", brandId);
                return NotFound($"Brand with ID {brandId} not found");
            }
            
            var result = await _storeService.GetStoresByBrandIdAsync(brandId.ToString());
            
            if (!result.Success)
            {
                _logger.LogWarning("Get stores for brand failed - {Error}", result.Errors);
                return BadRequest(result.Errors);
            }
            
            return Ok(result.Data);
        }

        /// <summary>
        /// Create a new store
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateStore([FromBody] CreateStoreDto request)
        {
            _logger.LogInformation("Admin creating store with name: {Name} for brand: {BrandId}", 
                request.Name, request.BrandId);
            
            // First verify brand exists
            var brandResult = await _brandService.GetBrandByIdAsync(request.BrandId);
            if (!brandResult.Success)
            {
                _logger.LogWarning("Brand not found ID: {BrandId}", request.BrandId);
                return BadRequest($"Brand with ID {request.BrandId} not found");
            }
            
            var result = await _storeService.CreateStoreAsync(request);
            
            if (!result.Success)
            {
                _logger.LogWarning("Admin create store failed - {Error}", result.Errors);
                return BadRequest(result.Errors);
            }
            
            return CreatedAtAction(nameof(GetStoreById), new { id = result.Data.Id }, result.Data);
        }

        /// <summary>
        /// Update an existing store
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStore([FromRoute] StoreId id, [FromBody] UpdateStoreDto request)
        {
            _logger.LogInformation("Admin updating store ID: {StoreId}", id);
            
            var result = await _storeService.UpdateStoreAsync(id.ToString(), request);
            
            if (!result.Success)
            {
                _logger.LogWarning("Admin update store failed - {Error}", result.Errors);
                return BadRequest(result.Errors);
            }
            
            return Ok(result.Data);
        }

        /// <summary>
        /// Find stores near a specific location
        /// </summary>
        [HttpGet("nearby")]
        public IActionResult GetNearbyStores(
            [FromQuery] double latitude, 
            [FromQuery] double longitude, 
            [FromQuery] double radiusKm = 10)
        {
            _logger.LogInformation("Admin searching for stores near location ({Latitude}, {Longitude}), radius: {RadiusKm}km", 
                latitude, longitude, radiusKm);
            
            if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
            {
                return BadRequest("Invalid coordinates");
            }
            
            if (radiusKm <= 0 || radiusKm > 100)
            {
                return BadRequest("Radius must be between 0 and 100 km");
            }
            
            // This functionality is not yet implemented in the StoreService
            // Return a message indicating this feature is coming soon
            return Ok(new 
            { 
                Message = "Nearby stores search functionality is not yet implemented.",
                PlannedFeature = true
            });
        }
    }
} 