namespace LoyaltySystem.Controllers;

using Microsoft.AspNetCore.Mvc;
using LoyaltySystem.Services;
using LoyaltySystem.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/businesses/{businessId}/members")]
public class MembersController : ControllerBase
{
    private readonly IMembersService _service;

    public MembersController(IMembersService service)
    {
        _service = service;
    }

    // GET /api/businesses/{businessId}/members
    [HttpGet]
    public async Task<ActionResult<List<Member>>> GetMembers(int businessId)
    {
        var list = await _service.GetMembersAsync(businessId);
        return Ok(list);
    }

    // GET /api/businesses/{businessId}/members/{memberId}
    [HttpGet("{memberId}")]
    public async Task<ActionResult<Member>> GetMember(int businessId, int memberId)
    {
        var member = await _service.GetMemberAsync(businessId, memberId);
        if (member == null) return NotFound("Member not found");
        return Ok(member);
    }

    // POST /api/businesses/{businessId}/members
    [HttpPost]
    public async Task<IActionResult> AddMember(int businessId, [FromBody] AddMemberDto dto)
    {
        var newId = await _service.AddMemberAsync(businessId, dto);
        return Ok(new { memberId = newId });
    }

    // PUT /api/businesses/{businessId}/members/{memberId}
    [HttpPut("{memberId}")]
    public async Task<IActionResult> UpdateMember(int businessId, int memberId, [FromBody] UpdateMemberDto dto)
    {
        await _service.UpdateMemberAsync(businessId, memberId, dto);
        return Ok("Member updated");
    }

    // DELETE /api/businesses/{businessId}/members/{memberId}
    [HttpDelete("{memberId}")]
    public async Task<IActionResult> DeleteMember(int businessId, int memberId)
    {
        await _service.DeleteMemberAsync(businessId, memberId);
        return Ok("Member deleted");
    }
}