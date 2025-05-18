using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.Social;
using LoyaltySystem.Application.Interfaces.Auth;
using LoyaltySystem.Application.Interfaces.Users;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Shared.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace LoyaltySystem.Staff.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseAuthController {

    public AuthController(
        IAuthenticationService authService,
        IUserService userService,
        ILogger logger
        )
        : base(
            authService,
            logger
        ) { }

    protected override string UserType => "Staff";
    
    protected override Task<OperationResult<RegisterUserResponseDto>> RegisterAsync(RegisterUserRequestDto registerRequest)
    {
        // Staff API should not allow user registration
        _logger.Warning("Attempt to register through Staff API blocked: {Email}", registerRequest.Email);
        return Task.FromResult(
            OperationResult<RegisterUserResponseDto>.FailureResult(
                new [] { "Registration not allowed through Staff API" }));
    }
    
    protected override Task<OperationResult<SocialAuthResponseDto>> SocialLoginInternalAsync(SocialAuthRequestDto request) =>
        throw new NotImplementedException();
}
