using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.PasswordReset;
using LoyaltySystem.Application.DTOs.Auth.Social;
using LoyaltySystem.Application.DTOs.Customer;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Application.Interfaces.Auth;
using LoyaltySystem.Application.Interfaces.Customers;
using LoyaltySystem.Application.Interfaces.Profile;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Shared.API.Controllers;
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
    private readonly IRegistrationService _registrationService;
    
    public AuthController(
        IAuthenticationService authService, 
        ICustomerService customerService,
        IProfileService profileService,
        IPasswordResetService passwordResetService,
        IEmailVerificationService emailVerificationService,
        ISocialAuthService socialAuthService,
        IRegistrationService registrationService,
        ILogger logger
    )
    : base(
        authService, 
        profileService,
        passwordResetService,
        emailVerificationService,
        logger
    )
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        _socialAuthService = socialAuthService ?? throw new ArgumentNullException(nameof(socialAuthService));
        _registrationService = registrationService ?? throw new ArgumentNullException(nameof(registrationService));
    }

    protected override string UserType => "Customer";
    protected override async Task<OperationResult<UserDto>> RegisterAsync(RegisterUserDto request) => 
        await _registrationService.RegisterUserAsync
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
            dto => _registrationService.RegisterUserAsync(dto, dto.Roles, createCustomer: true, customerData: new CustomerExtraData()) // TODO: Change this to use RegisterCustomerDto (which inherits RegisterUSerDto). Can we transform it?
        );

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