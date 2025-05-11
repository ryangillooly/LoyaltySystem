using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.Social;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Shared.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ILogger = Serilog.ILogger;

namespace LoyaltySystem.Staff.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseAuthController
{
    public AuthController(IAuthService authService, ILogger logger) 
        : base(authService, logger) { }

    protected override string UserType => "Staff";
    
    protected override Task<OperationResult<UserDto>> RegisterAsync(RegisterUserDto registerRequest)
    {
        // Staff API should not allow user registration
        _logger.Warning("Attempt to register through Staff API blocked: {Email}", registerRequest.Email);
        return Task.FromResult(
            OperationResult<UserDto>.FailureResult(
                new [] { "Registration not allowed through Staff API" }));
    }
    
    protected override async Task<OperationResult<ProfileDto>> GetProfileInternalAsync()
    {
        var userIdString = User.FindFirstValue("UserId");
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
    protected override Task<OperationResult<SocialAuthResponseDto>> SocialLoginInternalAsync(SocialAuthRequestDto request) =>
        throw new NotImplementedException();
    //protected override Task<OperationResult<bool>> ForgotPasswordInternalAsync(ForgotPasswordRequestDto request) =>
      //  throw new NotImplementedException();
    // protected override Task<OperationResult<bool>> ResetPasswordInternalAsync(ResetPasswordRequestDto request) =>
       // throw new NotImplementedException();
}
