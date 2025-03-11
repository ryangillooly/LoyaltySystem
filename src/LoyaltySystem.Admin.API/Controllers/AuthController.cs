using System;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Services;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Shared.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoyaltySystem.Admin.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseAuthController
    {
        public AuthController(AuthService authService, ILogger<AuthController> logger)
            : base(authService, logger)
        {
        }

        // Admin-specific authentication enhancements can be added here
        
        [Authorize(Roles = "Admin")]
        [HttpPost("users/roles/add")]
        public async Task<IActionResult> AddRole(UserRoleDto roleRequest)
        {
            _logger.LogInformation("Admin role add request: {UserId}, Role: {Role}", roleRequest.UserId, roleRequest.Role);
            
            if (!UserId.TryParse<UserId>(roleRequest.UserId, out var userId))
                return BadRequest(new { message = "Invalid user ID format" });
                
            if (!Enum.TryParse<RoleType>(roleRequest.Role, out var role))
                return BadRequest(new { message = "Invalid role type" });
                
            var result = await _authService.AddRoleAsync(userId, role);
            
            if (!result.Success)
            {
                _logger.LogWarning("Admin role add failed: {UserId}, Role: {Role} - {Error}", 
                    roleRequest.UserId, roleRequest.Role, result.Errors);
                return BadRequest(new { message = result.Errors });
            }
            
            _logger.LogInformation("Admin role added successfully: {UserId}, Role: {Role}", 
                roleRequest.UserId, roleRequest.Role);
            return Ok(result.Data);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("users/roles/remove")]
        public async Task<IActionResult> RemoveRole(UserRoleDto roleRequest)
        {
            _logger.LogInformation("Admin role remove request: {UserId}, Role: {Role}", 
                roleRequest.UserId, roleRequest.Role);
            
            if (!UserId.TryParse<UserId>(roleRequest.UserId, out var userId))
                return BadRequest(new { message = "Invalid user ID format" });
                
            if (!Enum.TryParse<RoleType>(roleRequest.Role, out var role))
                return BadRequest(new { message = "Invalid role type" });
                
            var result = await _authService.RemoveRoleAsync(userId, role);
            
            if (!result.Success)
            {
                _logger.LogWarning("Admin role remove failed: {UserId}, Role: {Role} - {Error}", 
                    roleRequest.UserId, roleRequest.Role, result.Errors);
                return BadRequest(new { message = result.Errors });
            }
            
            _logger.LogInformation("Admin role removed successfully: {UserId}, Role: {Role}", 
                roleRequest.UserId, roleRequest.Role);
            return Ok(result.Data);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("users/link-customer")]
        public async Task<IActionResult> LinkCustomer(LinkCustomerDto linkRequest)
        {
            _logger.LogInformation("Admin link customer request: {UserId}, CustomerId: {CustomerId}", 
                linkRequest.UserId, linkRequest.CustomerId);
            
            if (!UserId.TryParse<UserId>(linkRequest.UserId, out var userId))
                return BadRequest(new { message = "Invalid user ID format" });
                
            if (!CustomerId.TryParse<CustomerId>(linkRequest.CustomerId, out var customerId))
                return BadRequest(new { message = "Invalid customer ID format" });
                
            var result = await _authService.LinkCustomerAsync(userId, customerId);
            
            if (!result.Success)
            {
                _logger.LogWarning("Admin link customer failed: {UserId}, CustomerId: {CustomerId} - {Error}", 
                    linkRequest.UserId, linkRequest.CustomerId, result.Errors);
                return BadRequest(new { message = result.Errors });
            }
            
            _logger.LogInformation("Admin link customer successful: {UserId}, CustomerId: {CustomerId}", 
                linkRequest.UserId, linkRequest.CustomerId);
            return Ok(result.Data);
        }
    }
} 