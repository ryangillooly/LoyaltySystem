using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Application.Services;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Shared.API.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoyaltySystem.Shared.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseAuthController : ControllerBase
    {
        protected readonly IAuthService _authService;
        protected readonly ILogger _logger;

        protected BaseAuthController(IAuthService authService, ILogger logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("login")]
        public virtual async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            string identifier;
            LoginIdentifierType identifierType;
            
            if (!string.IsNullOrEmpty(loginRequest.Email))
            {
                identifier = loginRequest.Email;
                identifierType = LoginIdentifierType.Email;
                _logger.LogInformation("Login attempt using email: {Email}", identifier);
            }
            else if (!string.IsNullOrEmpty(loginRequest.UserName))
            {
                identifier = loginRequest.UserName;
                identifierType = LoginIdentifierType.Username;
                _logger.LogInformation("Login attempt using username: {Username}", identifier);
            }
            else
            {
                _logger.LogWarning("Login attempt with no identifier provided");
                return BadRequest(new { message = "Email or username must be provided" });
            }
            
            var result = await _authService.AuthenticateAsync(identifier, loginRequest.Password, identifierType);
            
            if (!result.Success)
            {
                _logger.LogWarning("Failed login attempt for: {Identifier}", identifier);
                return Unauthorized(new { message = result.Errors });
            }
            
            _logger.LogInformation("Successful login for: {Identifier}", identifier);
            return Ok(result.Data);
        }

        [HttpPost("register")]
        public virtual async Task<IActionResult> RegisterUser(RegisterUserDto registerRequest)
        {
            _logger.LogInformation("Registration attempt for email: {Email}", registerRequest.Email);
            
            if (registerRequest.Password != registerRequest.ConfirmPassword)
            {
                _logger.LogWarning("Registration failed - password mismatch for email: {Email}", registerRequest.Email);
                return BadRequest(new { message = "Password and confirmation password do not match" });
            }
            
            var result = await _authService.RegisterAsync(registerRequest);
            
            if (!result.Success)
            {
                _logger.LogWarning("Registration failed for email: {Email} - {Error}", registerRequest.Email, result.Errors);
                return BadRequest(new { message = result.Errors });
            }
            
            _logger.LogInformation("Successful registration for email: {Email}", registerRequest.Email);
            return CreatedAtAction(nameof(GetUserById), new { id = result.Data.Id }, result.Data);
        }

        [Authorize]
        [HttpGet("profile")]
        public virtual async Task<IActionResult> GetProfile()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Raw user ID from claim: {RawUserId}", userIdString);
            
            // Convert the string ID (with prefix) to a UserId object
            if (string.IsNullOrEmpty(userIdString) || !UserId.TryParse<UserId>(userIdString, out var userId))
            {
                _logger.LogWarning("Invalid user ID in token: {UserId}", userIdString);
                return Unauthorized(new { message = "Invalid user identification" });
            }
            
            var result = await _authService.GetUserByIdAsync(userId.ToString());
            
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
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Convert the string ID (with prefix) to a UserId object
            if (string.IsNullOrEmpty(userIdString) || !UserId.TryParse<UserId>(userIdString, out var userId))
            {
                _logger.LogWarning("Invalid user ID in token: {UserId}", userIdString);
                return Unauthorized(new { message = "Invalid user identification" });
            }
            
            _logger.LogInformation("Profile update request for user ID: {UserId}", userId);
            
            var result = await _authService.UpdateProfileAsync(userId.ToString(), updateRequest);
            
            if (!result.Success)
            {
                _logger.LogWarning("Profile update failed for user ID: {UserId} - {Error}", userId, result.Errors);
                return BadRequest(new { message = result.Errors });
            }
            
            _logger.LogInformation("Profile updated successfully for user ID: {UserId}", userId);
            return Ok(result.Data);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("users/{id}")]
        public virtual async Task<IActionResult> GetUserById(string id)
        {
            _logger.LogInformation("User detail request for ID: {UserId}", id);
            
            if (!UserId.TryParse<UserId>(id, out var userId))
            {
                _logger.LogWarning("Invalid user ID format: {UserId}", id);
                return BadRequest(new { message = "Invalid user ID format" });
            }
            
            var result = await _authService.GetUserByIdAsync(userId.ToString());
            
            if (!result.Success)
            {
                _logger.LogWarning("User detail request failed for ID: {UserId} - {Error}", id, result.Errors);
                return NotFound(new { message = result.Errors });
            }
            
            return Ok(result.Data);
        }
        
        protected UserId GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !UserId.TryParse<UserId>(userIdString, out var userId))
            {
                _logger.LogWarning("Invalid or missing user ID in claims: {ClaimValue}", userIdString);
                return null;
            }
            
            return userId;
        }
        
        protected CustomerId GetCurrentCustomerId()
        {
            var customerIdString = User.FindFirstValue("CustomerId");
            if (string.IsNullOrEmpty(customerIdString))
            {
                return null;
            }
            
            if (!CustomerId.TryParse<CustomerId>(customerIdString, out var customerId))
            {
                _logger.LogWarning("Invalid customer ID format in claims: {ClaimValue}", customerIdString);
                return null;
            }
            
            return customerId;
        }
    }
} 