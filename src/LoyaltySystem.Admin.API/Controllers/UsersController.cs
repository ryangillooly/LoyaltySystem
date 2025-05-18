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

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/> class with the specified user service and logger.
    /// </summary>
    /// <param name="userService">The service used for user profile operations.</param>
    /// <param name="logger">The logger for recording controller actions and errors.</param>
    public UsersController(IUserService userService, ILogger logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves the profile of a user specified by user ID. Accessible to users with SuperAdmin or Admin roles.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose profile is to be retrieved.</param>
    /// <returns>The user profile data if found; otherwise, a 404 Not Found response with error details.</returns>
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

    /// <summary>
    /// Updates a user's profile by user ID with the provided update data.
    /// </summary>
    /// <param name="userId">The ID of the user whose profile is to be updated.</param>
    /// <param name="updateRequestDto">The data to update the user's profile with.</param>
    /// <returns>An HTTP 200 response with the updated profile data if successful; otherwise, a 400 Bad Request with error details.</returns>
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
    
    /// <summary>
    /// Retrieves the profile of the currently authenticated user.
    /// </summary>
    /// <returns>The current user's profile data if found; otherwise, a BadRequest or NotFound result.</returns>
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

    /// <summary>
    /// Updates the profile of the currently authenticated user with the provided data.
    /// </summary>
    /// <param name="updateRequestDto">The profile update information.</param>
    /// <returns>An HTTP 200 response with the updated profile data on success; otherwise, a 400 response with error details.</returns>
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