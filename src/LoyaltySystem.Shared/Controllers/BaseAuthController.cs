using System.Security.Claims;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.PasswordReset;
using LoyaltySystem.Application.DTOs.Auth.Social;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Application.Interfaces.Auth;
using LoyaltySystem.Application.Interfaces.Profile;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using ILogger = Serilog.ILogger;

namespace LoyaltySystem.Shared.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseAuthController : ControllerBase 
{
    protected readonly IAuthenticationService _authService;
    protected readonly IPasswordResetService _passwordResetService;
    protected readonly IProfileService _profileService;
    protected readonly IEmailVerificationService _emailVerificationService;
    protected readonly ILogger _logger;
    
    protected BaseAuthController(
        IAuthenticationService authService,  
        IProfileService profileService,
        IPasswordResetService passwordResetService,
        IEmailVerificationService emailVerificationService,
        ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
        _passwordResetService = passwordResetService ?? throw new ArgumentNullException(nameof(passwordResetService));
        _emailVerificationService = emailVerificationService ?? throw new ArgumentNullException(nameof(emailVerificationService));
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
            case AuthIdentifierType.Email:
            case AuthIdentifierType.Username:
                _logger.Information("{UserType} login attempt using {IdentifierType}: {IdentifierValue}", UserType, request.IdentifierType, request.Identifier);
                break;

            default:
                _logger.Warning("{UserType} login attempt with no identifier provided", UserType);
                return OperationResult<AuthResponseDto>.FailureResult(new [] { "Email or username must be provided" });
        }
        
        return await _authService.AuthenticateAsync(request);
    }
    
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

    [AllowAnonymous]
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        var user = await _emailVerificationService.VerifyEmailAsync(token);
        if (!user.Success)
            return BadRequest(user.Errors);
        
        return Ok("Email verified successfully!");
    }
    
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
            
        var result = await _profileService.UpdateProfileAsync(userId.ToString(), updateRequest);
            
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
    
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public virtual async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        var result = await _passwordResetService.ForgotPasswordAsync(request);
        
        if (!result.Success)
            return BadRequest(new { message = result.Errors });
        
        return Ok(new { message = "If the account exists, a password reset email has been sent." });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public virtual async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        var result = await _passwordResetService.ResetPasswordAsync(request);
        
        if (!result.Success)
            return BadRequest(new { message = result.Errors });
        
        return Ok(new { message = "Password has been reset." });
    }
}