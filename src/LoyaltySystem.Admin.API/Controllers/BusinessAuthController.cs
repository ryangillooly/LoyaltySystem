using System.Security.Claims;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.Admin.API.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
public class BusinessAuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<BusinessAuthController> _logger;

    public BusinessAuthController(
        IAuthService authService,
        ILogger<BusinessAuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        _logger.LogInformation("Business login attempt");
            
        // Only allow business users (Admin, Manager)
        var result = await _authService.AuthenticateForAppAsync(
            GetIdentifier(request),
            request.Password,
            GetIdentifierType(request),
            new[] { RoleType.Admin, RoleType.Manager, RoleType.SuperAdmin });
                
        if (!result.Success)
        {
            _logger.LogWarning("Business login failed: {Error}", result.Errors);
            return Unauthorized(new { message = result.Errors });
        }
                
        _logger.LogInformation("Business login successful for user: {UserId}", result.Data.User.Id);
        return Ok(result.Data);
    }
        
    [HttpPost("register/admin")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> RegisterAdmin(RegisterUserDto request)
    {
        _logger.LogInformation("Admin registration attempt");
            
        // Validate password match
        if (request.Password != request.ConfirmPassword)
        {
            _logger.LogWarning("Admin registration failed - password mismatch");
            return BadRequest(new { message = "Password and confirmation password do not match" });
        }
            
        // Only super admins can create admins
        var result = await _authService.RegisterAdminAsync(request);
        if (!result.Success)
        {
            _logger.LogWarning("Admin registration failed: {Error}", result.Errors);
            return BadRequest(new { message = result.Errors });
        }
            
        _logger.LogInformation("Admin registration successful for: {Email}", request.Email);
        return CreatedAtAction(nameof(GetUserById), new { id = result.Data.Id }, result.Data);
    }
        
    [HttpPost("register/manager")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> RegisterManager(RegisterUserDto request)
    {
        _logger.LogInformation("Manager registration attempt");
            
        // Validate password match
        if (request.Password != request.ConfirmPassword)
        {
            _logger.LogWarning("Manager registration failed - password mismatch");
            return BadRequest(new { message = "Password and confirmation password do not match" });
        }
            
        // Register as admin but will assign manager role
        var result = await _authService.RegisterAdminAsync(request);
        if (!result.Success)
        {
            _logger.LogWarning("Manager registration failed: {Error}", result.Errors);
            return BadRequest(new { message = result.Errors });
        }
            
        // Change role from Admin to Manager
        await _authService.RemoveRoleAsync(result.Data.Id, RoleType.Admin);
        await _authService.AddRoleAsync(result.Data.Id, RoleType.Manager);
            
        _logger.LogInformation("Manager registration successful for: {Email}", request.Email);
        return CreatedAtAction(nameof(GetUserById), new { id = result.Data.Id }, result.Data);
    }
        
    [HttpPost("become-customer")]
    [Authorize(Roles = "Admin,Manager,Staff,SuperAdmin")]
    public async Task<IActionResult> BecomeCustomer()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
                
        _logger.LogInformation("Business user {UserId} requesting to become a customer", userId);
                
        // Add customer role to existing user
        var result = await _authService.AddCustomerRoleToUserAsync(userId);
        if (!result.Success)
        {
            _logger.LogWarning("Failed to add customer role: {Error}", result.Errors);
            return BadRequest(new { message = result.Errors });
        }
                
        _logger.LogInformation("Successfully added customer role to business user {UserId}", userId);
        return Ok(new { 
            message = "You can now also use the Customer App with your existing credentials",
            user = result.Data
        });
    }
        
    [Authorize(Roles = "SuperAdmin,Admin")]
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