using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Services;
using LoyaltySystem.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoyaltySystem.Shared.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseAuthController : ControllerBase
    {
        protected readonly AuthService _authService;
        protected readonly ILogger _logger;

        protected BaseAuthController(AuthService authService, ILogger logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("login")]
        public virtual async Task<IActionResult> Login(LoginRequestDto loginRequest)
        {
            _logger.LogInformation("Login attempt for user: {Username}", loginRequest.Username);
            
            var result = await _authService.AuthenticateAsync(loginRequest.Username, loginRequest.Password);
            
            if (!result.Success)
            {
                _logger.LogWarning("Failed login attempt for user: {Username}", loginRequest.Username);
                return Unauthorized(new { message = result.Errors });
            }
            
            _logger.LogInformation("Successful login for user: {Username}", loginRequest.Username);
            return Ok(result.Data);
        }

        [HttpPost("register")]
        public virtual async Task<IActionResult> Register(RegisterUserDto registerRequest)
        {
            _logger.LogInformation("Registration attempt for username: {Username}", registerRequest.Username);
            
            if (registerRequest.Password != registerRequest.ConfirmPassword)
            {
                _logger.LogWarning("Registration failed - password mismatch for username: {Username}", registerRequest.Username);
                return BadRequest(new { message = "Password and confirmation password do not match" });
            }
            
            var result = await _authService.RegisterAsync(registerRequest);
            
            if (!result.Success)
            {
                _logger.LogWarning("Registration failed for username: {Username} - {Error}", registerRequest.Username, result.Errors);
                return BadRequest(new { message = result.Errors });
            }
            
            _logger.LogInformation("Successful registration for user: {Username}", registerRequest.Username);
            return CreatedAtAction(nameof(GetUserById), new { id = result.Data.Id }, result.Data);
        }

        [Authorize]
        [HttpGet("profile")]
        public virtual async Task<IActionResult> GetProfile()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Raw user ID from claim: {RawUserId}", userIdString);
            
            // Create UserId from the raw GUID string (without prefix)
            UserId userId;
            if (Guid.TryParse(userIdString, out var userGuid))
            {
                userId = new UserId(userGuid);
                _logger.LogInformation("Parsed user ID from claim: {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("Invalid user ID format in claim: {InvalidId}", userIdString);
                return BadRequest(new { message = "Invalid user identity in token" });
            }
            
            var result = await _authService.GetUserByIdAsync(userId);
            
            if (!result.Success)
            {
                _logger.LogWarning("Profile request failed for user ID: {UserId} - {Error}", userId, result.Errors);
                return NotFound(new { message = result.Errors });
            }
            
            return Ok(result.Data);
        }

        [Authorize]
        [HttpPut("profile")]
        public virtual async Task<IActionResult> UpdateProfile(UpdateProfileDto updateRequest)
        {
            var userId = UserId.Parse<UserId>(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _logger.LogInformation("Profile update request for user ID: {UserId}", userId);
            
            var result = await _authService.UpdateProfileAsync(userId, updateRequest);
            
            if (!result.Success)
            {
                _logger.LogWarning("Profile update failed for user ID: {UserId} - {Error}", userId, result.Errors);
                return BadRequest(new { message = result.Errors });
            }
            
            _logger.LogInformation("Profile updated successfully for user ID: {UserId}", userId);
            return Ok(result.Data);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("users/{id}")]
        public virtual async Task<IActionResult> GetUserById(string id)
        {
            _logger.LogInformation("User detail request for ID: {UserId}", id);
            
            if (!UserId.TryParse<UserId>(id, out var userId))
            {
                _logger.LogWarning("Invalid user ID format: {UserId}", id);
                return BadRequest(new { message = "Invalid user ID format" });
            }
            
            var result = await _authService.GetUserByIdAsync(userId);
            
            if (!result.Success)
            {
                _logger.LogWarning("User detail request failed for ID: {UserId} - {Error}", id, result.Errors);
                return NotFound(new { message = result.Errors });
            }
            
            return Ok(result.Data);
        }
    }
} 