using System.Security.Claims;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.Customer.API.Controllers;

[ApiController]
[Route("api/customer/[controller]")]
public class CustomerAuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerAuthController> _logger;

    public CustomerAuthController(
        IAuthService authService,
        ICustomerService customerService,
        ILogger<CustomerAuthController> logger)
    {
        _authService = authService;
        _customerService = customerService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        _logger.LogInformation("Customer login attempt");
            
        // Only allow customer users
        var result = await _authService.AuthenticateForAppAsync(
            GetIdentifier(request),
            request.Password,
            GetIdentifierType(request),
            new[] { RoleType.Customer });
                
        if (!result.Success)
        {
            _logger.LogWarning("Customer login failed: {Error}", result.Errors);
            return Unauthorized(new { message = result.Errors });
        }
                
        _logger.LogInformation("Customer login successful for user: {UserId}", result.Data.User.Id);
        return Ok(result.Data);
    }
        
    [HttpPost("register")]
    public async Task<IActionResult> RegisterCustomer(RegisterUserDto request)
    {
        _logger.LogInformation("Customer registration attempt");
            
        // Validate password match
        if (request.Password != request.ConfirmPassword)
        {
            _logger.LogWarning("Customer registration failed - password mismatch");
            return BadRequest(new { message = "Password and confirmation password do not match" });
        }
            
        // Use the customer-specific registration
        var result = await _authService.RegisterCustomerAsync(request);
        if (!result.Success)
        {
            _logger.LogWarning("Customer registration failed: {Error}", result.Errors);
            return BadRequest(new { message = result.Errors });
        }
            
        _logger.LogInformation("Customer registration successful for: {Email}", request.Email);
        return CreatedAtAction(nameof(GetProfile), null, result.Data);
    }
        
    [HttpPost("register/enhanced")]
    public async Task<IActionResult> RegisterEnhancedCustomer(CustomerRegisterDto request)
    {
        _logger.LogInformation("Enhanced customer registration attempt");
            
        // Validate password match
        if (request.Password != request.ConfirmPassword)
        {
            _logger.LogWarning("Enhanced customer registration failed - password mismatch");
            return BadRequest(new { message = "Password and confirmation password do not match" });
        }
            
        // Register the customer
        var result = await _authService.RegisterCustomerAsync(request);
        if (!result.Success)
        {
            _logger.LogWarning("Enhanced customer registration failed: {Error}", result.Errors);
            return BadRequest(new { message = result.Errors });
        }
            
        // Update customer with additional information
        var customerId = result.Data.CustomerId;
        if (!string.IsNullOrEmpty(customerId))
        {
            var updateResult = await _customerService.UpdateCustomerAsync(
                customerId, 
                new UpdateCustomerDto 
                { 
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Phone = request.Phone,
                    Address = request.Address,
                    DateOfBirth = request.DateOfBirth,
                    MarketingConsent = request.MarketingConsent
                });
                
            if (!updateResult.Success)
            {
                _logger.LogWarning("Customer profile update failed: {Error}", updateResult.Errors);
                // Continue anyway since user is created
            }
        }
            
        _logger.LogInformation("Enhanced customer registration successful for: {Email}", request.Email);
        return CreatedAtAction(nameof(GetProfile), null, result.Data);
    }
        
    [Authorize(Roles = "Customer")]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
            
        var result = await _authService.GetUserByIdAsync(userId);
            
        if (!result.Success)
            return NotFound(new { message = result.Errors });
                
        return Ok(result.Data);
    }
        
    [Authorize(Roles = "Customer")]
    [HttpGet("loyalty-summary")]
    public async Task<IActionResult> GetLoyaltySummary()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var customerIdClaim = User.FindFirstValue("CustomerId");
            
        if (string.IsNullOrEmpty(customerIdClaim))
            return BadRequest(new { message = "User not linked to a customer" });
            
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
        
    #region Helper Methods
        
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
        
    #endregion
}