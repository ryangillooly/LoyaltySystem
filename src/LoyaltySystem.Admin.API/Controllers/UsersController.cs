using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Application.Interfaces.Users;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Shared.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;
using System.Security.Claims;

namespace LoyaltySystem.Admin.API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger _logger;

    public UsersController(IUserService userService, ILogger logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Authorize(Roles = "SuperAdmin, Admin")]
    [HttpGet("{userId}/profile")]
    public async Task<IActionResult> GetUserProfile([FromRoute] string userId)
    {
        var result = await _userService.GetByIdAsync(UserId.FromString(userId));
        if (!result.Success)
        {
            _logger.Warning("Admin failed to get user profile for {UserId}: {Error}", userId, result.Errors);
            return NotFound(result.Errors);
        }
        return Ok(result.Data);
    }

    [Authorize(Roles = "SuperAdmin, Admin")]
    [HttpPut("{userId}/profile")]
    public async Task<IActionResult> UpdateUserProfile([FromRoute] string userId, [FromBody] UpdateUserRequestDto updateRequestDto)
    {
        var result = await _userService.UpdateAsync(UserId.FromString(userId), updateRequestDto);
        if (!result.Success)
        {
            _logger.Warning("Admin failed to update user profile for {UserId}: {Error}", userId, result.Errors);
            return BadRequest(result.Errors);
        }
        return Ok(result.Data);
    }
    
    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetCurrentUserProfile()
    {
        if (!User.TryGetUserId(out var userId))
        {
            _logger.Warning("Invalid customer ID in token: {CustomerId}", User.FindFirstValue("UserId"));
            return BadRequest("Invalid customer ID in token");
        }
        
        var result = await _userService.GetByIdAsync(userId);
        if (!result.Success)
        {
            _logger.Warning("Admin failed to get user profile for {UserId}: {Error}", userId, result.Errors);
            return NotFound(result.Errors);
        }
        return Ok(result.Data);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateCurrentUserProfile([FromBody] UpdateUserRequestDto updateRequestDto)
    {
        if (!User.TryGetUserId(out var userId))
        {
            _logger.Warning("Invalid customer ID in token: {CustomerId}", User.FindFirstValue("UserId"));
            return BadRequest("Invalid customer ID in token");
        }
        
        var result = await _userService.UpdateAsync(userId, updateRequestDto);
        if (!result.Success)
        {
            _logger.Warning("Admin failed to update user profile for {UserId}: {Error}", userId, result.Errors);
            return BadRequest(result.Errors);
        }
        
        _logger.Information("Profile updated for user Id: {UserId}", userId.ToString());
        return Ok(result.Data);
    }
} 