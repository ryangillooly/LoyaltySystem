using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.Social;
using LoyaltySystem.Application.DTOs.Customers;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Application.Interfaces.Auth;
using LoyaltySystem.Application.Interfaces.Customers;
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
    private readonly IAccountService _accountService;
    
    public AuthController(
        IAuthenticationService authService, 
        ICustomerService customerService,
        ISocialAuthService socialAuthService,
        IAccountService accountService,
        ILogger logger
    )
    : base(
        authService, 
        logger
    )
    {
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        _socialAuthService = socialAuthService ?? throw new ArgumentNullException(nameof(socialAuthService));
    }

    protected override string UserType => "Customer";
    protected override async Task<OperationResult<InternalUserDto>> RegisterAsync(RegisterUserDto request) => 
        await _accountService.RegisterAsync
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
            dto => _accountService.RegisterAsync(dto, dto.Roles, createCustomer: true, customerData: new CustomerExtraData()) // TODO: Change this to use RegisterCustomerDto (which inherits RegisterUSerDto). Can we transform it?
        );
}