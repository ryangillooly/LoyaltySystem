using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Shared.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoyaltySystem.Customer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseAuthController
    {
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
            : base(authService, logger)
        {
        }

        // Override register to automatically assign customer role
        [HttpPost("register")]
        public override async Task<IActionResult> Register(RegisterUserDto registerRequest)
        {
            _logger.LogInformation("Customer registration attempt for username: {Username}", registerRequest.Username);
            
            if (registerRequest.Password != registerRequest.ConfirmPassword)
            {
                _logger.LogWarning("Customer registration failed - password mismatch for username: {Username}", registerRequest.Username);
                return BadRequest(new { message = "Password and confirmation password do not match" });
            }
            
            var result = await _authService.RegisterAsync(registerRequest);
            
            if (!result.Success)
            {
                _logger.LogWarning("Customer registration failed for username: {Username} - {Error}", registerRequest.Username, result.Errors);
                return BadRequest(new { message = result.Errors });
            }
            
            // Ensure the user has the Customer role
            await _authService.AddRoleAsync(result.Data.Id, RoleType.Customer);
            
            _logger.LogInformation("Successful customer registration for user: {Username}", registerRequest.Username);
            return CreatedAtAction(nameof(GetProfile), null, result.Data);
        }
        
        // Add login endpoint for customer authentication
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            _logger.LogInformation("Customer login attempt for username: {Username}", loginRequest.Username);
            
            var result = await _authService.AuthenticateAsync(loginRequest.Username, loginRequest.Password);
            
            if (!result.Success)
            {
                _logger.LogWarning("Customer login failed for username: {Username} - {Error}", loginRequest.Username, result.Errors);
                return Unauthorized(new { message = result.Errors });
            }
            
            // Verify the user has the Customer role
            if (!await HasRole(result.Data.UserId, RoleType.Customer))
            {
                _logger.LogWarning("Non-customer user attempted to log in through customer API: {Username}", loginRequest.Username);
                return Unauthorized(new { message = "Access denied. This API is for customers only." });
            }
            
            _logger.LogInformation("Successful customer login for user: {Username}", loginRequest.Username);
            return Ok(result.Data);
        }
        
        // Helper method to check if a user has a specific role
        private async Task<bool> HasRole(string userId, RoleType role)
        {
            // Get the user details
            var userResult = await _authService.GetUserByIdAsync(userId);
            if (!userResult.Success)
                return false;
                
            // Check if the user has the specified role
            return userResult.Data.Roles.Contains(role.ToString());
        }
        
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("users/{id}")]
        public override async Task<IActionResult> GetUserById(string id)
        {
            // This should not be accessible in the customer API
            return StatusCode(403, new { message = "Access denied" });
        }
        
        // Add customer-specific endpoints
        
        [Authorize]
        [HttpGet("loyalty-summary")]
        public async Task<IActionResult> GetLoyaltySummary()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customerIdClaim = User.FindFirstValue("CustomerId");
            
            if (string.IsNullOrEmpty(customerIdClaim))
            {
                return BadRequest(new { message = "User not linked to a customer" });
            }
            
            var customerId = CustomerId.Parse<CustomerId>(customerIdClaim);
            
            // In a real implementation, we would call a service to get the loyalty summary
            // For now, return a placeholder response
            return Ok(new
            {
                customerId = customerId.ToString(),
                activeProgramCount = 2,
                totalPoints = 150,
                totalStamps = 8,
                pendingRewards = 1
            });
        }
    }
} 