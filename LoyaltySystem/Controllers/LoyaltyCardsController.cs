namespace LoyaltySystem.Controllers;

using Microsoft.AspNetCore.Mvc;
using LoyaltySystem.Services;
using LoyaltySystem.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/businesses/{businessId}/loyalty-cards")]
public class LoyaltyCardsController : ControllerBase
{
    private readonly ILoyaltyCardService _service;

    public LoyaltyCardsController(ILoyaltyCardService service)
    {
        _service = service;
    }

    // ============== Templates ==============
    // GET /api/businesses/{businessId}/loyalty-cards/templates
    [HttpGet("templates")]
    public async Task<ActionResult<List<LoyaltyCardTemplate>>> GetTemplates(int businessId)
    {
        var list = await _service.GetTemplatesAsync(businessId);
        return Ok(list);
    }

    // POST /api/businesses/{businessId}/loyalty-cards/templates
    [HttpPost("templates")]
    public async Task<IActionResult> CreateTemplate(int businessId, [FromBody] CreateTemplateDto dto)
    {
        var newId = await _service.CreateTemplateAsync(businessId, dto);
        return Ok(new { loyaltyCardTemplateId = newId });
    }

    // GET /api/businesses/{businessId}/loyalty-cards/templates/{templateId}
    [HttpGet("templates/{templateId}")]
    public async Task<ActionResult<LoyaltyCardTemplate>> GetTemplate(int businessId, int templateId)
    {
        var template = await _service.GetTemplateAsync(businessId, templateId);
        if (template == null) return NotFound("Template not found");
        return Ok(template);
    }

    // PUT /api/businesses/{businessId}/loyalty-cards/templates/{templateId}
    [HttpPut("templates/{templateId}")]
    public async Task<IActionResult> UpdateTemplate(int businessId, int templateId, [FromBody] UpdateTemplateDto dto)
    {
        await _service.UpdateTemplateAsync(businessId, templateId, dto);
        return Ok("Template updated");
    }

    // DELETE /api/businesses/{businessId}/loyalty-cards/templates/{templateId}
    [HttpDelete("templates/{templateId}")]
    public async Task<IActionResult> DeleteTemplate(int businessId, int templateId)
    {
        await _service.DeleteTemplateAsync(businessId, templateId);
        return Ok("Template deleted");
    }

    // ============== User Loyalty Cards ==============
    // GET /api/businesses/{businessId}/loyalty-cards/users
    [HttpGet("users")]
    public async Task<ActionResult<List<UserLoyaltyCard>>> GetUserCards(int businessId)
    {
        var list = await _service.GetUserCardsAsync(businessId);
        return Ok(list);
    }

    // POST /api/businesses/{businessId}/loyalty-cards/users
    [HttpPost("users")]
    public async Task<IActionResult> CreateUserCard(int businessId, [FromBody] CreateUserCardDto dto)
    {
        var newId = await _service.CreateUserCardAsync(businessId, dto);
        return Ok(new { userLoyaltyCardId = newId });
    }

    // GET /api/businesses/{businessId}/loyalty-cards/users/{cardId}
    [HttpGet("users/{cardId}")]
    public async Task<ActionResult<UserLoyaltyCard>> GetUserCard(int businessId, int cardId)
    {
        var card = await _service.GetUserCardAsync(businessId, cardId);
        if (card == null) return NotFound("User card not found");
        return Ok(card);
    }

    // PUT /api/businesses/{businessId}/loyalty-cards/users/{cardId}
    [HttpPut("users/{cardId}")]
    public async Task<IActionResult> UpdateUserCard(int businessId, int cardId, [FromBody] UpdateUserCardDto dto)
    {
        await _service.UpdateUserCardAsync(businessId, cardId, dto);
        return Ok("User card updated");
    }

    // DELETE /api/businesses/{businessId}/loyalty-cards/users/{cardId}
    [HttpDelete("users/{cardId}")]
    public async Task<IActionResult> DeleteUserCard(int businessId, int cardId)
    {
        await _service.DeleteUserCardAsync(businessId, cardId);
        return Ok("User card deleted");
    }
}
