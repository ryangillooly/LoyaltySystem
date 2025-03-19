using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Shared.API.Controllers;
using LoyaltySystem.Shared.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.Admin.API.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
public class AuthController : BaseAuthController
{
    public AuthController(IAuthService authService, ILogger<AuthController> logger)
        : base(authService, logger)
    {
    }
    
    [HttpPost("register/admin")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterUserDto registerDto)
    {
        _logger.LogInformation("Admin registration attempt for email: {Email}", registerDto.Email);
            
        if (registerDto.Password != registerDto.ConfirmPassword)
        {
            _logger.LogWarning("Admin registration failed: Passwords don't match for {Email}", registerDto.Email);
            return BadRequest(new { message = "Password and confirmation password do not match" });
        }

        var result = await _authService.RegisterAdminAsync(registerDto);
            
        if (!result.Success)
        {
            _logger.LogWarning("Admin registration failed for {Email}: {Error}", registerDto.Email, string.Join(", ", result.Errors));
            return BadRequest(new { message = result.Errors });
        }

        _logger.LogInformation("Admin registration successful for {Email}", registerDto.Email);
        return CreatedAtAction(nameof(GetUserById), new { id = result.Data.Id }, result.Data);
    }
        
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("users/roles/add")]
    public async Task<IActionResult> AddRole(UserRoleDto roleRequest)
    {
        _logger.LogInformation("Admin role add request: {UserId}, Role: {Role}", roleRequest.UserId, roleRequest.Role);
            
        // Parse the user ID using the extension method
        var parseResult = this.TryParseId(roleRequest.UserId, out UserId userId, _logger, "UserId");
        if (parseResult != null)
            return parseResult;
                
        if (!Enum.TryParse<RoleType>(roleRequest.Role, out var role))
            return BadRequest(new { message = "Invalid role type" });
                
        var result = await _authService.AddRoleAsync(userId.Value.ToString(), role);
            
        if (!result.Success)
        {
            _logger.LogWarning("Admin role add failed: {UserId}, Role: {Role} - {Error}", 
                userId, roleRequest.Role, result.Errors);
            return BadRequest(new { message = result.Errors });
        }
            
        _logger.LogInformation("Admin role added successfully: {UserId}, Role: {Role}", 
            userId, roleRequest.Role);
        return Ok(result.Data);
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("users/roles/remove")]
    public async Task<IActionResult> RemoveRole(UserRoleDto roleRequest)
    {
        _logger.LogInformation("Admin role remove request: {UserId}, Role: {Role}", 
            roleRequest.UserId, roleRequest.Role);
            
        // Parse the user ID using the extension method
        var parseResult = this.TryParseId(roleRequest.UserId, out UserId userId, _logger, "UserId");
        if (parseResult != null)
            return parseResult;
                
        if (!Enum.TryParse<RoleType>(roleRequest.Role, out var role))
            return BadRequest(new { message = "Invalid role type" });
                
        var result = await _authService.RemoveRoleAsync(userId.ToString(), role);
            
        if (!result.Success)
        {
            _logger.LogWarning("Admin role remove failed: {UserId}, Role: {Role} - {Error}", 
                userId, roleRequest.Role, result.Errors);
            return BadRequest(new { message = result.Errors });
        }
            
        _logger.LogInformation("Admin role removed successfully: {UserId}, Role: {Role}", 
            userId, roleRequest.Role);
        return Ok(result.Data);
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("users/link-customer")]
    public async Task<IActionResult> LinkCustomer(LinkCustomerDto linkRequest)
    {
        _logger.LogInformation("Admin link customer request: {UserId}, CustomerId: {CustomerId}", 
            linkRequest.UserId, linkRequest.CustomerId);
            
        ArgumentNullException.ThrowIfNull(linkRequest.CustomerId);
        ArgumentNullException.ThrowIfNull(linkRequest.UserId);
            
        // Parse the user ID using the extension method
        var userIdResult = this.TryParseId(linkRequest.UserId, out UserId userId, _logger, "UserId");
        if (userIdResult != null)
            return userIdResult;
            
        // Parse the customer ID using the extension method
        var customerIdResult = this.TryParseId(linkRequest.CustomerId, out CustomerId customerId, _logger, "CustomerId");
        if (customerIdResult != null)
            return customerIdResult;
                
        var result = await _authService.LinkCustomerAsync(userId.ToString(), customerId.ToString());
            
        if (!result.Success)
        {
            _logger.LogWarning("Admin link customer failed: {UserId}, {CustomerId} - {Error}",
                userId, customerId, result.Errors);
            return BadRequest(new { message = result.Errors });
        }
            
        _logger.LogInformation("Customer linked successfully: {UserId}, {CustomerId}",
            userId, customerId);
        return Ok(result.Data);
    }
}