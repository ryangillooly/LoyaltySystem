using System.Security.Claims;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.Social;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace LoyaltySystem.Shared.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseAuthController : ControllerBase
{
    protected readonly IAuthService _authService;
    protected readonly ILogger _logger;
    
    protected BaseAuthController(IAuthService authService, ILogger logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected abstract string UserType { get; }
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await AuthenticateAsync(request);
        if (!result.Success)
        {
            _logger
                .ForContext("Errors", result.Errors)
                .Warning("{UserType} login failed for {Type} {Id}", UserType, request.IdentifierType.ToString(), request.Identifier);
            
            return Unauthorized(new { message = result.Errors });
        }
                
        _logger.Information("Successful {UserType} login for {Type} {Id}", UserType, request.IdentifierType.ToString(), request.Identifier);
        return Ok(result.Data);
    }
    protected virtual async Task<OperationResult<AuthResponseDto>> AuthenticateAsync(LoginRequestDto request)
    {
        switch (request.IdentifierType)
        {
            case LoginIdentifierType.Email:
            case LoginIdentifierType.Username:
                _logger.Information("{UserType} login attempt using {IdentifierType}: {IdentifierValue}", UserType, request.IdentifierType, request.Identifier);
                break;

            default:
                _logger.Warning("{UserType} login attempt with no identifier provided", UserType);
                return OperationResult<AuthResponseDto>.FailureResult(new [] { "Email or username must be provided" });
        }
            
        return await _authService.AuthenticateAsync(request.Identifier, request.Password, request.IdentifierType);
    }

    
    [AllowAnonymous]
    [HttpPost("register")]
    public virtual async Task<IActionResult> Register(RegisterUserDto request)
    {
        _logger.Information("{UserType} registration attempt for email: {Email}", UserType, request.Email);
            
        if (request.Password != request.ConfirmPassword)
        {
            _logger.Warning("{UserType} registration failed - password mismatch for email: {Email}", UserType, request.Email);
            return BadRequest(new { message = "Password and confirmation password do not match" });
        }
            
        // Call the appropriate registration method for the specific controller type
        var result = await RegisterAsync(request);
            
        if (!result.Success)
        {
            _logger.Warning("{UserType} registration failed for email: {Email} - {Error}", UserType, request.Email, result.Errors);
            return BadRequest(new { message = result.Errors });
        }
            
        _logger.Information("Successful {UserType} registration for email: {Email}", UserType, request.Email);
        return CreatedAtAction(nameof(GetProfile), result.Data);
    }
    protected abstract Task<OperationResult<UserDto>> RegisterAsync(RegisterUserDto registerRequest);
    
    // TODO: Add Customer Profile as well as user profile info
    [Authorize]
    [HttpGet("profile")]
    public virtual async Task<IActionResult> GetProfile()
    {
        var result = await GetProfileInternalAsync();
        if (!result.Success)
        {
            _logger.Warning("Profile request failed: {Error}", result.Errors);
            return NotFound(new { message = result.Errors });
        }
            
        return Ok(result.Data);
    }
    protected abstract Task<OperationResult<ProfileDto>> GetProfileInternalAsync();

    [Authorize]
    [HttpPut("/users/{userId}/profile")]
    public virtual async Task<IActionResult> UpdateProfile(UpdateProfileDto updateRequest)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
        // Convert the string ID (with prefix) to a UserId object
        if (string.IsNullOrEmpty(userIdString) || !UserId.TryParse<UserId>(userIdString, out var userId))
        {
            _logger.Warning("Invalid user ID in token: {UserId}", userIdString);
            return Unauthorized(new { message = "Invalid user identification" });
        }
            
        _logger.Information("Profile update request for user ID: {UserId}", userId);
            
        var result = await _authService.UpdateProfileAsync(userId.ToString(), updateRequest);
            
        if (!result.Success)
        {
            _logger.Warning("Profile update failed for user ID: {UserId} - {Error}", userId, result.Errors);
            return BadRequest(new { message = result.Errors });
        }
            
        _logger.Information("Profile updated successfully for user ID: {UserId}", userId);
        return Ok(result.Data);
    }
    
    [AllowAnonymous]
    [HttpPost("social-login")]
    public virtual async Task<IActionResult> SocialLogin([FromBody] SocialAuthRequestDto request)
    {
        var result = await SocialLoginInternalAsync(request);
        if (!result.Success)
            return Unauthorized(new { message = result.Errors });
        return Ok(result.Data);
    }
    protected abstract Task<OperationResult<SocialAuthResponseDto>> SocialLoginInternalAsync(SocialAuthRequestDto request);

    /*
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public virtual async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        var result = await ForgotPasswordInternalAsync(request);
        if (!result.Success)
            return BadRequest(new { message = result.Errors });
        return Ok(new { message = "If the account exists, a password reset email has been sent." });
    }
    protected abstract Task<OperationResult<bool>> ForgotPasswordInternalAsync(ForgotPasswordRequestDto request);

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public virtual async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        var result = await ResetPasswordInternalAsync(request);
        if (!result.Success)
            return BadRequest(new { message = result.Errors });
        return Ok(new { message = "Password has been reset." });
    }
    protected abstract Task<OperationResult<bool>> ResetPasswordInternalAsync(ResetPasswordRequestDto request);
    */
}