using System.Security.Claims;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Shared.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.Customer.API.Controllers;

[ApiController]
[Route("api/customer/[controller]")]
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
    
    // Override the template method to explicitly specify customer registration
    // This is technically redundant since the base class already does this,
    // but we include it for clarity and to follow LSP properly
    protected override async Task<OperationResult<UserDto>> ExecuteRegistrationAsync(RegisterUserDto registerRequest)
    {
        // In the customer controller, we register as a customer user (same as base)
        return await _authService.RegisterCustomerAsync(registerRequest);
    }
    
    [HttpPost("register")]
    public override async Task<IActionResult> RegisterUser(RegisterUserDto registerRequest)
    {
        _logger.LogInformation("Customer registration attempt for email: {email}", registerRequest.Email);
            
        if (registerRequest.Password != registerRequest.ConfirmPassword)
        {
            _logger.LogWarning("Customer registration failed - password mismatch for email: {email}", registerRequest.Email);
            return BadRequest(new { message = "Password and confirmation password do not match" });
        }
            
        var result = await _authService.RegisterCustomerAsync(registerRequest);
            
        if (!result.Success)
        {
            _logger.LogWarning("Customer registration failed for email: {email} - {errors}", registerRequest.Email, result.Errors);
            return BadRequest(new { message = result.Errors });
        }
            
        // Ensure the user has the Customer role
        await _authService.AddRoleAsync(result.Data.PrefixedId, RoleType.Customer);
            
        _logger.LogInformation("Successful customer registration for email: {email}", registerRequest.Email);
        return CreatedAtAction(nameof(GetProfile), null, result.Data);
    }
    
    [HttpPost("register/customer")]
    public async Task<IActionResult> RegisterCustomer(CustomerRegisterDto registerRequest)
    {
        _logger.LogInformation("Enhanced customer registration attempt for email: {email}", registerRequest.Email);
            
        if (registerRequest.Password != registerRequest.ConfirmPassword)
        {
            _logger.LogWarning("Customer registration failed - password mismatch for email: {email}", registerRequest.Email);
            return BadRequest(new { message = "Password and confirmation password do not match" });
        }
            
        var result = await _authService.RegisterCustomerAsync(registerRequest);
            
        if (!result.Success)
        {
            _logger.LogWarning("Customer registration failed for email: {email} - {errors}", registerRequest.Email, result.Errors);
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
        var userResult = await _authService.GetUserByIdAsync(result.Data.PrefixedId);
        if (!userResult.Success || userResult.Data.CustomerId == null)
        {
            _logger.LogWarning("Customer data creation failed for user: {UserId}", result.Data.PrefixedId);
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
    
    [HttpPost("login")]
    public override async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
    {
        // Determine which identifier to use and its type
        string identifier;
        LoginIdentifierType identifierType;
            
        if (!string.IsNullOrEmpty(loginRequest.Email))
        {
            identifier = loginRequest.Email;
            identifierType = LoginIdentifierType.Email;
            _logger.LogInformation("Customer login attempt using email: {email}", identifier);
        }
        else if (!string.IsNullOrEmpty(loginRequest.UserName))
        {
            identifier = loginRequest.UserName;
            identifierType = LoginIdentifierType.Username;
            _logger.LogInformation("Customer login attempt using username: {username}", identifier);
        }
        else
        {
            _logger.LogWarning("Customer login attempt with no identifier provided");
            return BadRequest(new { message = "Email or username must be provided" });
        }
            
        var result = await _authService.AuthenticateAsync(identifier, loginRequest.Password, identifierType);
            
        if (!result.Success)
        {
            _logger.LogWarning("Customer login failed for: {identifier} - {Error}", identifier, result.Errors);
            return Unauthorized(new { message = result.Errors });
        }
            
        _logger.LogInformation("Successful customer login for: {identifier}", identifier);
        return Ok(result.Data);
    }
        
    // Helper method to check if a user has a specific role
    private async Task<bool> HasRoles(string userPrefixedId, List<RoleType> roles)
    {
        // Get the user details using the prefixed ID string
        var userResult = await _authService.GetUserByIdAsync(userPrefixedId);
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
}