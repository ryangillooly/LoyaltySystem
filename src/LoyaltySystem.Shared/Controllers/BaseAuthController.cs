
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.Social;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Application.Interfaces.Auth;
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
    private readonly IAuthenticationService _authService;
    protected readonly ILogger _logger;
    
    protected BaseAuthController(
        IAuthenticationService authService,
        ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService)); 
    }

    protected abstract string UserType { get; }
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        _logger.Information("{UserType} login attempt using {Type}: {Id}", UserType, request.IdentifierType, request.Identifier);        
        
        var result = await _authService.AuthenticateAsync(request);
        if (!result.Success)
        {
            _logger
                .ForContext("Errors", result.Errors)
                .Warning("{UserType} login failed for {Type} {Id}", UserType, request.IdentifierType.ToString(), request.Identifier);
            
            return Unauthorized(new { message = result.Errors });
        }
                
        _logger.Information("Successful {UserType} login for {Type} {Id}", UserType, request.IdentifierType, request.Identifier);
        return Ok(result.Data);
    }
    
    protected virtual async Task<OperationResult<LoginResponseDto>> AuthenticateAsync(LoginRequestDto request)
    {
        switch (request.IdentifierType)
        {
            case AuthIdentifierType.Email:
            case AuthIdentifierType.Username:
                _logger.Information("{UserType} login attempt using {IdentifierType}: {IdentifierValue}", UserType, request.IdentifierType, request.Identifier);
                break;

            default:
                _logger.Warning("{UserType} login attempt with no identifier provided", UserType);
                return OperationResult<LoginResponseDto>.FailureResult(new [] { "Email or username must be provided" });
        }
        
        return await _authService.AuthenticateAsync(request);
    }
    
    
    [HttpPost("register")]
    public virtual async Task<IActionResult> Register(RegisterUserRequestDto request)
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
        return CreatedAtAction(
            actionName: "GetUserProfile",
            controllerName: "Users",
            routeValues: new { userId = result.Data.Id },
            value: result.Data
        );
    }
    protected abstract Task<OperationResult<RegisterUserResponseDto>> RegisterAsync(RegisterUserRequestDto registerRequest);
    
    
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
    

}