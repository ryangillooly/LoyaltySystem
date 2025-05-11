using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.PasswordReset;
using LoyaltySystem.Application.DTOs.Auth.Social;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Shared.API.Controllers;
using LoyaltySystem.Shared.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ILogger = Serilog.ILogger;

namespace LoyaltySystem.Admin.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseAuthController 
{
    private readonly ISocialAuthService _socialAuthService;
    public AuthController(
        IAuthService authService, 
        ISocialAuthService socialAuthService, 
        ILogger logger
    )
        : base(authService, logger) => 
            _socialAuthService = socialAuthService;

    protected override string UserType => "Admin";
    protected override Task<OperationResult<UserDto>> RegisterAsync(RegisterUserDto registerRequest) =>
        throw new NotImplementedException();

    protected override async Task<OperationResult<SocialAuthResponseDto>> SocialLoginInternalAsync(SocialAuthRequestDto request) =>
        await _socialAuthService.AuthenticateAsync(
            request,
            new[] { RoleType.Admin, RoleType.Manager, RoleType.User },
            dto => _authService.RegisterUserAsync(dto, dto.Roles, createCustomer: false, customerData: null)
        );

    private const string UserId = "UserId";
    
    [HttpPost("register")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public override async Task<IActionResult> Register(RegisterUserDto request)
    {
        _logger.Information("{UserType} registration attempt for email: {Email}", UserType, request.Email);
            
        if (request.Password != request.ConfirmPassword)
        {
            _logger.Warning("{UserType} registration failed - password mismatch for email: {Email}", UserType, request.Email);
            return BadRequest(new { message = "Password and confirmation password do not match" });
        }
        
        if (!request.Roles.Any())
            return BadRequest(new { message = "Role is required" });

        // Only SuperAdmin can create Admins
        if ((request.Roles.Contains(RoleType.Admin) || request.Roles.Contains(RoleType.SuperAdmin))
            && !User.IsInRole(nameof(RoleType.SuperAdmin)))
            return Forbid();
        
        // Admins or SuperAdmins can create Managers
        if (request.Roles.Contains(RoleType.Manager) && 
            !(User.IsInRole(nameof(RoleType.SuperAdmin)) || User.IsInRole(nameof(RoleType.Admin))))
            return Forbid();

        // Register the user (using admin registration logic for both roles)
        var result = await _authService.RegisterUserAsync(request, new [] { RoleType.Admin });
        if (!result.Success)
            return BadRequest(new { message = result.Errors });

        if (!result.Success)
        {
            _logger.Warning("{UserType} registration failed for email: {Email} - {Error}", UserType, request.Email, result.Errors);
            return BadRequest(new { message = result.Errors });
        }
            
        _logger.Information("Successful {UserType} registration for email: {Email}", UserType, request.Email);
        return CreatedAtAction(nameof(GetProfile), result.Data);
    }
        
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("users/{userId}/roles")]
    public async Task<IActionResult> AddRole([FromRoute] string userId, [FromBody] RoleType roleType)
    {
        _logger.Information("Admin role add request: {UserId}, Role: {Role}", userId, roleType);
            
        // Parse the user ID using the extension method
        if (!EntityId.TryParse<UserId>(userId, out _))
        {
            _logger.Warning("Invalid user ID in route: {UserId}", userId);
            return NotFound(new { message = "User Id not found" });
        }
                
        var result = await _authService.AddRoleAsync(userId, roleType);
            
        if (!result.Success)
        {
            _logger.Warning("Admin role add failed: {UserId}, Role: {Role} - {Error}", userId, roleType, result.Errors);
            return BadRequest(new { message = result.Errors });
        }
            
        _logger.Information("Admin role added successfully: {UserId}, Role: {Role}", userId, roleType);
        return Ok(result.Data);
    }

    protected override async Task<OperationResult<ProfileDto>> GetProfileInternalAsync()
    {
        var userIdString = User.FindFirstValue(UserId);
        if (string.IsNullOrEmpty(userIdString) || !EntityId.TryParse<UserId>(userIdString, out var userId))
        {
            _logger.Warning("Invalid customer ID in token: {CustomerId}", userIdString);
            return OperationResult<ProfileDto>.FailureResult("Invalid customer identification");
        }

        var result = await _authService.GetUserByIdAsync(userIdString);
        return result.Success
            ? OperationResult<ProfileDto>.SuccessResult(result.Data!)
            : OperationResult<ProfileDto>.FailureResult(result.Errors!);
    }
    
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpDelete("users/{userId}/roles")]
    public async Task<IActionResult> RemoveRole([FromRoute] string userId, [FromBody] RoleType roleType)
    {
        _logger.Information("Admin role remove request: {UserId}, Role: {Role}", userId, nameof(roleType));
        
        // Parse the user ID using the extension method
        if (!EntityId.TryParse<UserId>(userId, out _))
        {
            _logger.Warning("Invalid user ID in route: {UserId}", userId);
            return NotFound(new { message = "User Id not found" });
        }
        
        var result = await _authService.RemoveRoleAsync(userId, roleType);
            
        if (!result.Success)
        {
            _logger.Warning("Admin role remove failed: {UserId}, Role: {Role} - {Error}", userId, nameof(roleType), result.Errors);
            return BadRequest(new { message = result.Errors });
        }
            
        _logger.Information("Admin role removed successfully: {UserId}, Role: {Role}", userId, nameof(roleType));
        return Ok(result.Data);
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("users/link-customer")]
    public async Task<IActionResult> LinkCustomer(LinkCustomerDto linkRequest)
    {
        _logger.Information("Admin link customer request: {UserId}, CustomerId: {CustomerId}", 
            linkRequest.UserId, linkRequest.CustomerId);
            
        ArgumentNullException.ThrowIfNull(linkRequest.CustomerId);
        ArgumentNullException.ThrowIfNull(linkRequest.UserId);
            
        // Parse the user ID using the extension method
        var userIdResult = this.TryParseId(linkRequest.UserId, out UserId userId, _logger, UserId);
        if (userIdResult != null)
            return userIdResult;
            
        // Parse the customer ID using the extension method
        var customerIdResult = this.TryParseId(linkRequest.CustomerId, out CustomerId customerId, _logger, "CustomerId");
        if (customerIdResult != null)
            return customerIdResult;
                
        var result = await _authService.LinkCustomerAsync(userId.ToString(), customerId.ToString());
            
        if (!result.Success)
        {
            _logger.Warning("Admin link customer failed: {UserId}, {CustomerId} - {Error}",
                userId, customerId, result.Errors);
            return BadRequest(new { message = result.Errors });
        }
            
        _logger.Information("Customer linked successfully: {UserId}, {CustomerId}",
            userId, customerId);
        return Ok(result.Data);
    }
}
