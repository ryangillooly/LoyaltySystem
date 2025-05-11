using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.Social;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Application.DTOs.Customer;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Shared.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ILogger = Serilog.ILogger;

namespace LoyaltySystem.Customer.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseAuthController 
{
    private readonly ISocialAuthService _socialAuthService;
    private readonly ICustomerService _customerService;
    public AuthController(
        IAuthService authService, 
        ICustomerService customerService, 
        ISocialAuthService socialAuthService, 
        ILogger logger)
        : base(authService, logger)
    {
        _customerService = customerService;
        _socialAuthService = socialAuthService;
    }

    protected override string UserType => "Customer";
    protected override async Task<OperationResult<UserDto>> RegisterAsync(RegisterUserDto request) => 
        await _authService.RegisterUserAsync
        (
            request, 
            roles: new [] { RoleType.Customer },
            createCustomer: true,
            new CustomerExtraData() // TODO: Change this to use RegisterCustomerDto (which inherits RegisterUSerDto). Can we transform it?
        );

    protected override async Task<OperationResult<SocialAuthResponseDto>> SocialLoginInternalAsync(SocialAuthRequestDto request) =>
        await _socialAuthService.AuthenticateAsync(
            request,
            new[] { RoleType.Customer },
            dto => _authService.RegisterUserAsync(dto, dto.Roles, createCustomer: true, customerData: new CustomerExtraData()) // TODO: Change this to use RegisterCustomerDto (which inherits RegisterUSerDto). Can we transform it?
        );
    // protected override Task<OperationResult<bool>> ForgotPasswordInternalAsync(ForgotPasswordRequestDto request) =>
       // throw new NotImplementedException();
    // protected override Task<OperationResult<bool>> ResetPasswordInternalAsync(ResetPasswordRequestDto request) =>
       // throw new NotImplementedException();

    protected override async Task<OperationResult<ProfileDto>> GetProfileInternalAsync()
    {
        var customerIdString = User.FindFirstValue("CustomerId");
        if (string.IsNullOrEmpty(customerIdString) || !EntityId.TryParse<CustomerId>(customerIdString, out var customerId))
        {
            _logger.Warning("Invalid customer ID in token: {CustomerId}", customerIdString);
            return OperationResult<ProfileDto>.FailureResult("Invalid customer identification");
        }

        var result = await _customerService.GetCustomerByIdAsync(customerIdString);
        return result.Success
            ? OperationResult<ProfileDto>.SuccessResult(result.Data!)
            : OperationResult<ProfileDto>.FailureResult(result.Errors!);
    }
}