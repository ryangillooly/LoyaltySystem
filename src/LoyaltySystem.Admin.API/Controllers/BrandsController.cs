
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.Admin.API.Controllers;

[ApiController]
[Route("api/brands")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class BrandsController : ControllerBase
{
    private readonly IBrandService _brandService;
    private readonly ILogger<BrandsController> _logger;

    public BrandsController(
        IBrandService brandService,
        ILogger<BrandsController> logger)
    {
        _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all brands with pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllBrands([FromQuery] int skip = 0, [FromQuery] int limit = 50)
    {
        _logger.LogInformation("Admin requesting all brands (skip: {skip}, limit: {limit})", 
            skip, limit);
            
        if (limit < 1 || skip < 0)
            return BadRequest("Invalid pagination parameters");
            
        var result = await _brandService.GetAllBrandsAsync(skip, limit);
            
        if (!result.Success)
        {
            _logger.LogWarning("Get all brands failed - {Error}", result.Errors);
            return BadRequest(result.Errors);
        }
            
        return Ok(result.Data);
    }

    /// <summary>
    /// Get a specific brand by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBrandById([FromRoute] BrandId id)
    {
        _logger.LogInformation("Admin requesting brand by ID: {BrandId}", id);
            
        var result = await _brandService.GetBrandByIdAsync(id.ToString());
            
        if (!result.Success)
        {
            _logger.LogWarning("Get brand failed for ID: {BrandId} - {Error}", id, result.Errors);
            return NotFound(result.Errors);
        }
            
        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new brand
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateBrand([FromBody] CreateBrandDto request)
    {
        _logger.LogInformation("Admin creating brand with name: {Name}", request.Name);
            
        var result = await _brandService.CreateBrandAsync(request);
            
        if (!result.Success)
        {
            _logger.LogWarning("Admin create brand failed - {Error}", result.Errors);
            return BadRequest(result.Errors);
        }
            
        return CreatedAtAction(nameof(GetBrandById), new { id = result.Data.Id }, result.Data);
    }

    /// <summary>
    /// Update an existing brand
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBrand([FromRoute] BrandId id, [FromBody] UpdateBrandDto request)
    {
        _logger.LogInformation("Admin updating brand ID: {BrandId}", id);
            
        var result = await _brandService.UpdateBrandAsync(id.ToString(), request);
            
        if (!result.Success)
        {
            _logger.LogWarning("Admin update brand failed - {Error}", result.Errors);
            return BadRequest(result.Errors);
        }
            
        return Ok(result.Data);
    }
}