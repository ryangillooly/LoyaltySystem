using LoyaltySystem.Application.DTOs.AuthDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoyaltySystem.Application.Interfaces.Users;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Staff.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Staff,Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _usersService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _usersService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("{userId}/profile")]
    public async Task<IActionResult> GetUserProfile(string userId)
    {
        var result = await _usersService.GetByIdAsync(UserId.FromString(userId));
        if (!result.Success)
        {
            _logger.LogWarning("Staff failed to get user profile for {UserId}: {Error}", userId, result.Errors);
            return NotFound(result.Errors);
        }
        return Ok(result.Data);
    }

    [HttpPut("{userId}/profile")]
    public async Task<IActionResult> UpdateUserProfile(string userId, [FromBody] UpdateUserDto updateDto)
    {
        var result = await _usersService.UpdateAsync(UserId.FromString(userId), updateDto);
        if (!result.Success)
        {
            _logger.LogWarning("Staff failed to update user profile for {UserId}: {Error}", userId, result.Errors);
            return BadRequest(result.Errors);
        }
        return Ok(result.Data);
    }
} 