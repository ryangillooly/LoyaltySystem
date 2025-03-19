using LoyaltySystem.Application.DTOs.LoyaltyPrograms;
using LoyaltySystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.Admin.API.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class LoyaltyProgramsController : ControllerBase
{
    private readonly ILoyaltyProgramService _programService;
    private readonly ILogger<LoyaltyProgramsController> _logger;

    public LoyaltyProgramsController(
        ILoyaltyProgramService programService,
        ILogger<LoyaltyProgramsController> logger)
    {
        _programService = programService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetProgramsAsync([FromQuery] string brandId = null, [FromQuery] string type = null, [FromQuery] int skip = 0, [FromQuery] int limit = 50)
    {
        try
        {
            if (!string.IsNullOrEmpty(brandId))
            {
                var result = await _programService.GetProgramsByBrandIdAsync(brandId);
                return result.Success
                    ? Ok(result.Data)
                    : BadRequest(result.Errors);
            }
                
            if (!string.IsNullOrEmpty(type))
            {
                var result = await _programService.GetProgramsByTypeAsync(type, skip, limit);
                return result.Success
                    ? Ok(result.Data)
                    : BadRequest(result.Errors);
            }
            else
            {
                var result = await _programService.GetAllProgramsAsync(null, skip, limit);
                return result.Success
                    ? Ok(result.Data)
                    : BadRequest(result.Errors);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving loyalty programs");
            return StatusCode(500, "An error occurred while retrieving loyalty programs");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProgramAsync(string id)
    {
        try
        {
            var result = await _programService.GetProgramByIdAsync(id);
            return result.Success
                ? Ok(result.Data)
                : NotFound(result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving loyalty program");
            return StatusCode(500, "An error occurred while retrieving the loyalty program");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateProgramAsync([FromBody] CreateLoyaltyProgramDto dto)
    {
        try
        {
            var result = await _programService.CreateProgramAsync(dto);
                
            return result.Success
                ? CreatedAtAction(nameof(GetProgramAsync), new { id = result.Data.Id }, result.Data)
                : BadRequest(result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating loyalty program");
            return StatusCode(500, "An error occurred while creating the loyalty program");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProgramAsync(string id, [FromBody] UpdateLoyaltyProgramDto dto)
    {
        try
        {
            var result = await _programService.UpdateProgramAsync(id, dto);
            return result.Success
                ? Ok(result.Data)
                : BadRequest(result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating loyalty program");
            return StatusCode(500, "An error occurred while updating the loyalty program");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProgramAsync(string id)
    {
        try
        {
            var result = await _programService.DeleteProgramAsync(id);
            return result.Success
                ? NoContent()
                : BadRequest(result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting loyalty program");
            return StatusCode(500, "An error occurred while deleting the loyalty program");
        }
    }
        

    public class CreateProgramDto
    {
        public Guid BrandId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public ExpirationPolicyRequestDto ExpirationPolicy { get; set; }
    }
        
    public class ExpirationPolicyRequestDto
    {
        public bool HasExpiration { get; set; }
        public bool ExpiresOnSpecificDate { get; set; }
        public int? ExpirationType { get; set; }
        public int? ExpirationValue { get; set; }
        public int? ExpirationDay { get; set; }
        public int? ExpirationMonth { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int? DurationInDays { get; set; }
    }
}