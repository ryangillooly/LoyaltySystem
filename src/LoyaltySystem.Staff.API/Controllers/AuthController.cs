using System;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Services;
using LoyaltySystem.Shared.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoyaltySystem.Staff.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseAuthController
    {
        public AuthController(AuthService authService, ILogger<AuthController> logger) 
            : base(authService, logger)
        {
        }

        // Override the registration method to prevent customer registration through staff API
        [HttpPost("register")]
        public override async Task<IActionResult> Register(RegisterUserDto registerRequest)
        {
            // Staff API should not allow customer registration
            _logger.LogWarning("Attempt to register through Staff API blocked: {Username}", registerRequest.Username);
            return StatusCode(403, new { message = "Registration not allowed through Staff API" });
        }
        
        // Staff-specific endpoints
        
        [Authorize(Roles = "Staff,StoreManager,BrandManager,Admin")]
        [HttpGet("validate-staff")]
        public IActionResult ValidateStaffCredentials()
        {
            // This endpoint exists just to validate that the user has staff access
            return Ok(new { message = "Valid staff credentials" });
        }
    }
} 