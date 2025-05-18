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
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class for handling customer authentication requests.
    /// </summary>
    /// <param name="authService">Authentication service for user authentication operations.</param>
    /// <param name="customerService">Service for customer-specific operations.</param>
    /// <param name="socialAuthService">Service for handling social authentication.</param>
    /// <param name="accountService">Service for account management and registration.</param>
    /// <param name="logger">Logger for recording controller activity.</param>
    /// <exception cref="ArgumentNullException">Thrown if any required service dependency is null.</exception>
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
    /// <summary>
        /// Registers a new customer account with the specified registration details.
        /// </summary>
        /// <param name="request">The registration information for the new customer.</param>
        /// <returns>The result of the registration operation, including user details if successful.</returns>
        protected override async Task<OperationResult<RegisterUserResponseDto>> RegisterAsync(RegisterUserRequestDto request) => 
        await _accountService.RegisterAsync
        (
            request, 
            roles: new [] { RoleType.Customer },
            createCustomer: true,
            new CustomerExtraData() // TODO: Change this to use RegisterCustomerDto (which inherits RegisterUSerDto). Can we transform it?
        );

    /// <summary>
        /// Handles social login for customers by authenticating with the provided social credentials and registering a new customer account if necessary.
        /// </summary>
        /// <param name="request">The social authentication request containing provider and credential information.</param>
        /// <returns>The result of the social authentication attempt, including user details if successful.</returns>
        protected override async Task<OperationResult<SocialAuthResponseDto>> SocialLoginInternalAsync(SocialAuthRequestDto request) =>
        await _socialAuthService.AuthenticateAsync(
            request,
            new[] { RoleType.Customer },
            dto => _accountService.RegisterAsync(dto, dto.Roles, createCustomer: true, customerData: new CustomerExtraData()) // TODO: Change this to use RegisterCustomerDto (which inherits RegisterUSerDto). Can we transform it?
        );
}