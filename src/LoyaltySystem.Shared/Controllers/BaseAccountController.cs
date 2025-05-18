using LoyaltySystem.Application.DTOs.Auth.EmailVerification;
using LoyaltySystem.Application.DTOs.Auth.PasswordReset;
using LoyaltySystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace LoyaltySystem.Shared.API.Controllers;

[AllowAnonymous]
[ApiController]
public abstract class BaseAccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger _logger;

    protected BaseAccountController(
        IAccountService accountService,
        ILogger logger)
    {
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    [HttpPost("verify-email")]
    public virtual async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        var result = await _accountService.VerifyEmailAsync(token);
        if (!result.Success)
            return BadRequest(result.Errors);
        
        return Ok("Email verified successfully!");
    }

    [HttpPost("resend-verification")]
    public virtual async Task<IActionResult> ResendVerificationEmail([FromBody] ResendEmailVerificationRequestDto request)
    {
        var result = await _accountService.ResendVerificationEmailAsync(request.Email);
        if (!result.Success)
            _logger.Warning("Resend verification failed for {Email}: {Error}", request.Email, result.Errors);
        else
            _logger.Information("Verification email resent to {Email}", request.Email);
        
        return Ok(new { message = "If the account exists, a verification email has been sent." });
    }
    
    [HttpPost("forgot-password")]
    public virtual async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        var result = await _accountService.ForgotPasswordAsync(request);
        
        if (!result.Success)
            return BadRequest(new { message = result.Errors });
        
        return Ok(new { message = "If the account exists, a password reset email has been sent." });
    }
    
    [HttpPost("reset-password")]
    public virtual async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        var result = await _accountService.ResetPasswordAsync(request);
        
        if (!result.Success)
            return BadRequest(new { message = result.Errors });
        
        return Ok(new { message = "Password has been reset." });
    }
}