using System.Security.Claims;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Shared.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace LoyaltySystem.Staff.API.Controllers
{
    [ApiController]
    [Route("api/staff/[controller]")]
    public class StaffAuthController : BaseAuthController
    {
        public StaffAuthController(
            IAuthService authService,
            ILogger<StaffAuthController> logger)
            : base(authService, logger)
        {
        }
        
        // Override the template method to specify staff registration instead of customer registration
        protected override async Task<OperationResult<UserDto>> ExecuteRegistrationAsync(RegisterUserDto registerRequest)
        {
            // In the staff controller, we register as a staff user, not a customer
            return await _authService.RegisterStaffAsync(registerRequest);
        }

        [HttpPost("login")]
        public override async Task<IActionResult> Login(LoginRequestDto request)
        {
            _logger.LogInformation("Staff login attempt");
            
            // Only allow staff users
            var result = await _authService.AuthenticateForAppAsync(
                GetIdentifier(request),
                request.Password,
                GetIdentifierType(request),
                new[] { RoleType.Staff });
                
            if (!result.Success)
            {
                _logger.LogWarning("Staff login failed: {Error}", result.Errors);
                return Unauthorized(new { message = result.Errors });
            }
                
            _logger.LogInformation("Staff login successful for user: {UserId}", result.Data.User.Id);
            return Ok(result.Data);
        }
        
        private string GetIdentifier(LoginRequestDto request)
        {
            return !string.IsNullOrEmpty(request.Email) 
                ? request.Email 
                : request.UserName;
        }
        
        private LoginIdentifierType GetIdentifierType(LoginRequestDto request)
        {
            return !string.IsNullOrEmpty(request.Email) 
                ? LoginIdentifierType.Email 
                : LoginIdentifierType.Username;
        }
        
        [HttpPost("register")]
        [Authorize(Roles = "Admin,Manager,SuperAdmin")]
        public async Task<IActionResult> RegisterStaff(RegisterUserDto request)
        {
            _logger.LogInformation("Staff registration attempt");
            
            // Validate password match
            if (request.Password != request.ConfirmPassword)
            {
                _logger.LogWarning("Staff registration failed - password mismatch");
                return BadRequest(new { message = "Password and confirmation password do not match" });
            }
            
            // Only managers/admins can register staff
            var result = await _authService.RegisterStaffAsync(request);
            if (!result.Success)
            {
                _logger.LogWarning("Staff registration failed: {Error}", result.Errors);
                return BadRequest(new { message = result.Errors });
            }
            
            _logger.LogInformation("Staff registration successful for: {Email}", request.Email);
            return CreatedAtAction(nameof(GetUserById), new { id = result.Data.Id }, result.Data);
        }
        
        [HttpPost("become-customer")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> BecomeCustomer()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
                
            _logger.LogInformation("Staff user {UserId} requesting to become a customer", userId);
                
            // Add customer role to existing user
            var result = await _authService.AddCustomerRoleToUserAsync(userId);
            if (!result.Success)
            {
                _logger.LogWarning("Failed to add customer role: {Error}", result.Errors);
                return BadRequest(new { message = result.Errors });
            }
                
            _logger.LogInformation("Successfully added customer role to staff user {UserId}", userId);
            return Ok(new { 
                message = "You can now also use the Customer App with your existing credentials",
                user = result.Data
            });
        }
        
        // Override the profile endpoint to add Staff role authorization
        [Authorize(Roles = "Staff")]
        [HttpGet("profile")]
        public override async Task<IActionResult> GetProfile()
        {
            // Call the base implementation to reuse its logic
            return await base.GetProfile();
        }
        
        [Authorize(Roles = "Admin,Manager,SuperAdmin")]
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            _logger.LogInformation("User detail request for ID: {UserId}", id);
            
            var result = await _authService.GetUserByIdAsync(id);
            
            if (!result.Success)
            {
                _logger.LogWarning("User detail request failed for ID: {UserId} - {Error}", id, result.Errors);
                return NotFound(new { message = result.Errors });
            }
            
            return Ok(result.Data);
        }
    }
} 