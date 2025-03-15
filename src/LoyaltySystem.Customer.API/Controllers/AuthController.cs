using System.Security.Claims;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Shared.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.Customer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseAuthController
    {
        private readonly ICustomerService _customerService;

        public AuthController(
            IAuthService authService, 
            ICustomerService customerService,
            ILogger<AuthController> logger)
            : base(authService, logger)
        {
            _customerService = customerService;
        }

        // Override register to automatically assign customer role
        [HttpPost("register")]
        public override async Task<IActionResult> RegisterUser(RegisterUserDto registerRequest)
        {
            _logger.LogInformation("Customer registration attempt for email: {email}", registerRequest.Email);
            
            if (registerRequest.Password != registerRequest.ConfirmPassword)
            {
                _logger.LogWarning("Customer registration failed - password mismatch for email: {email}", registerRequest.Email);
                return BadRequest(new { message = "Password and confirmation password do not match" });
            }
            
            var result = await _authService.RegisterAsync(registerRequest);
            
            if (!result.Success)
            {
                _logger.LogWarning("Customer registration failed for email: {email} - {errors}", registerRequest.Email, result.Errors);
                return BadRequest(new { message = result.Errors });
            }
            
            // Ensure the user has the Customer role
            await _authService.AddRoleAsync(result.Data.Id, RoleType.Customer);
            
            _logger.LogInformation("Successful customer registration for email: {email}", registerRequest.Email);
            return CreatedAtAction(nameof(GetProfile), null, result.Data);
        }
        
        // Enhanced customer registration endpoint
        [HttpPost("register/customer")]
        public async Task<IActionResult> RegisterCustomer(CustomerRegisterDto registerRequest)
        {
            _logger.LogInformation("Enhanced customer registration attempt for email: {email}", registerRequest.Email);
            
            if (registerRequest.Password != registerRequest.ConfirmPassword)
            {
                _logger.LogWarning("Customer registration failed - password mismatch for email: {email}", registerRequest.Email);
                return BadRequest(new { message = "Password and confirmation password do not match" });
            }
            
            var result = await _authService.RegisterAsync(registerRequest);
            if (!result.Success)
            {
                _logger.LogWarning("Customer registration failed for username: email: {email} - {errors}", registerRequest.Email, result.Errors);
                return BadRequest(new { message = result.Errors });
            }
            
            // Create or update Customer entity with additional details
            var createCustomerDto = new CreateCustomerDto
            {
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName,
                Email = registerRequest.Email,
                Phone = registerRequest.Phone,
                Address = registerRequest.Address,
                DateOfBirth = registerRequest.DateOfBirth,
                MarketingConsent = registerRequest.MarketingConsent
            };
            
            // Get the customer ID from the user
            var userResult = await _authService.GetUserByIdAsync(result.Data.Id);
            if (!userResult.Success || userResult.Data.CustomerId == null)
            {
                _logger.LogWarning("Customer data creation failed for user: {UserId}", result.Data.Id);
                return BadRequest(new { message = "Failed to create customer profile" });
            }
            
            // Update the customer with additional information
            var updateResult = await _customerService.UpdateCustomerAsync(
                userResult.Data.CustomerId, 
                new UpdateCustomerDto 
                { 
                    FirstName = registerRequest.FirstName,
                    LastName = registerRequest.LastName,
                    Email = registerRequest.Email,
                    Phone = registerRequest.Phone,
                    Address = registerRequest.Address,
                    DateOfBirth = registerRequest.DateOfBirth,
                    MarketingConsent = registerRequest.MarketingConsent
                });
            
            if (!updateResult.Success)
            {
                _logger.LogWarning("Customer profile update failed: {Error}", updateResult.Errors);
                // Continue anyway since user is created
            }
            
            _logger.LogInformation("Successful enhanced customer registration for email: {email}", registerRequest.Email);
            return CreatedAtAction(nameof(GetProfile), null, result.Data);
        }
        
        // Add login endpoint for customer authentication
        [HttpPost("login")]
        public override async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            _logger.LogInformation("Customer login attempt for email: {email}", loginRequest.Email);
            
            var result = await _authService.AuthenticateAsync(loginRequest.Email, loginRequest.Password);
            
            if (!result.Success)
            {
                _logger.LogWarning("Customer login failed for email: {email} - {Error}", loginRequest.Email, result.Errors);
                return Unauthorized(new { message = result.Errors });
            }
            
            // Verify the user has the Customer role
            if (!await HasRoles(result.Data.User.Id, new List<RoleType> { RoleType.Customer, RoleType.SuperAdmin }))
            {
                _logger.LogWarning("Non-customer user attempted to log in through customer API: {email}", loginRequest.Email);
                return Unauthorized(new { message = "Access denied. This API is for customers only." });
            }
            
            _logger.LogInformation("Successful customer login for email: {email}", loginRequest.Email);
            return Ok(result.Data);
        }
        
        // Helper method to check if a user has a specific role
        private async Task<bool> HasRoles(string userId, List<RoleType> roles)
        {
            // Get the user details
            var userResult = await _authService.GetUserByIdAsync(userId);
            if (!userResult.Success)
                return false;
                
            // Check if the user has the required roles
            return roles.Any(role => 
                userResult.Data.Roles.Contains(role.ToString()));
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