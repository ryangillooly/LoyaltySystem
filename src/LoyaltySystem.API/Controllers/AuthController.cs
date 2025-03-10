using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Services;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto loginRequest)
        {
            var result = await _authService.AuthenticateAsync(loginRequest.Username, loginRequest.Password);
            
            if (!result.Success)
                return Unauthorized(new { message = result.Errors });
                
            return Ok(result.Data);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDto registerRequest)
        {
            if (registerRequest.Password != registerRequest.ConfirmPassword)
                return BadRequest(new { message = "Password and confirmation password do not match" });
                
            var result = await _authService.RegisterAsync(registerRequest);
            
            if (!result.Success)
                return BadRequest(new { message = result.Errors });
                
            return CreatedAtAction(nameof(GetUserById), new { id = result.Data.Id }, result.Data);
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = UserId.Parse<UserId>(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var result = await _authService.GetUserByIdAsync(userId);
            
            if (!result.Success)
                return NotFound(new { message = result.Errors });
                
            return Ok(result.Data);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto updateRequest)
        {
            var userId = UserId.Parse<UserId>(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var result = await _authService.UpdateProfileAsync(userId, updateRequest);
            
            if (!result.Success)
                return BadRequest(new { message = result.Errors });
                
            return Ok(result.Data);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            if (!UserId.TryParse<UserId>(id, out var userId))
                return BadRequest(new { message = "Invalid user ID format" });
                
            var result = await _authService.GetUserByIdAsync(userId);
            
            if (!result.Success)
                return NotFound(new { message = result.Errors });
                
            return Ok(result.Data);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("users/roles/add")]
        public async Task<IActionResult> AddRole(UserRoleDto roleRequest)
        {
            if (!UserId.TryParse<UserId>(roleRequest.UserId, out var userId))
                return BadRequest(new { message = "Invalid user ID format" });
                
            if (!Enum.TryParse<RoleType>(roleRequest.Role, out var role))
                return BadRequest(new { message = "Invalid role type" });
                
            var result = await _authService.AddRoleAsync(userId, role);
            
            if (!result.Success)
                return BadRequest(new { message = result.Errors });
                
            return Ok(result.Data);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("users/roles/remove")]
        public async Task<IActionResult> RemoveRole(UserRoleDto roleRequest)
        {
            if (!UserId.TryParse<UserId>(roleRequest.UserId, out var userId))
                return BadRequest(new { message = "Invalid user ID format" });
                
            if (!Enum.TryParse<RoleType>(roleRequest.Role, out var role))
                return BadRequest(new { message = "Invalid role type" });
                
            var result = await _authService.RemoveRoleAsync(userId, role);
            
            if (!result.Success)
                return BadRequest(new { message = result.Errors });
                
            return Ok(result.Data);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("users/link-customer")]
        public async Task<IActionResult> LinkCustomer(LinkCustomerDto linkRequest)
        {
            if (!UserId.TryParse<UserId>(linkRequest.UserId, out var userId))
                return BadRequest(new { message = "Invalid user ID format" });
                
            if (!CustomerId.TryParse<CustomerId>(linkRequest.CustomerId, out var customerId))
                return BadRequest(new { message = "Invalid customer ID format" });
                
            var result = await _authService.LinkCustomerAsync(userId, customerId);
            
            if (!result.Success)
                return BadRequest(new { message = result.Errors });
                
            return Ok(result.Data);
        }
    }
} 