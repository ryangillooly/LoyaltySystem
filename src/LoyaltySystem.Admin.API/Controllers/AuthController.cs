using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.Social;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Application.Interfaces.Auth;
using LoyaltySystem.Application.Interfaces.Customers;
using LoyaltySystem.Application.Interfaces.Roles;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Shared.API.Controllers;
using LoyaltySystem.Shared.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace LoyaltySystem.Admin.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseAuthController 
{
    private readonly ISocialAuthService _socialAuthService;
    private readonly ICustomerService _customerService;
    private readonly IAccountService _accountService;
    private readonly IRolesService _rolesService;
    protected override string UserType => "Admin";
    private const string UserId = "UserId";
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class, configuring authentication, social authentication, customer management, account management, role management, and logging services for admin operations.
    /// </summary>
    public AuthController(
        IAuthenticationService authService,
        ISocialAuthService socialAuthService,
        ICustomerService customerService,
        IAccountService accountService,
        IRolesService rolesService,
        ILogger logger
    )
    : base(
        authService, 
        logger
    )
    {
        _rolesService = rolesService ?? throw new ArgumentNullException(nameof(rolesService));
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        _socialAuthService = socialAuthService ?? throw new ArgumentNullException(nameof(socialAuthService));
    }
    
    /// <summary>
    /// Registers a new admin or manager user with specified roles. Accessible only to users with SuperAdmin or Admin roles.
    /// </summary>
    /// <param name="request">The registration details for the new user.</param>
    /// <returns>An IActionResult indicating the outcome of the registration.</returns>
    [HttpPost("register")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public override async Task<IActionResult> Register(RegisterUserRequestDto request) => await base.Register(request);
    /// <summary>
    /// Registers a new user with specified roles, enforcing that only SuperAdmins can assign admin roles and only Admins or SuperAdmins can assign manager roles.
    /// </summary>
    /// <param name="request">The registration request containing user details and desired roles.</param>
    /// <returns>An operation result containing the registration response or error details if role assignment rules are violated or registration fails.</returns>
    protected override async Task<OperationResult<RegisterUserResponseDto>> RegisterAsync(RegisterUserRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        // Only SuperAdmin can create Admins
        var adminRolesRequested = request.Roles.Any(role => role >= RoleType.Admin);
        var managerRoleRequested = request.Roles.Contains(RoleType.Manager);
        var userIsSuperAdmin = User.IsInRole(nameof(RoleType.SuperAdmin));
        var userIsAnAdmin = User.IsInRole(nameof(RoleType.Admin)) || User.IsInRole(nameof(RoleType.SuperAdmin));
        
        if (!userIsSuperAdmin && adminRolesRequested)
            return await Task.FromResult(
                OperationResult<RegisterUserResponseDto>.FailureResult(new[] { "SuperAdmin role required to assign admin roles" }, OperationErrorType.Forbidden));
        
        // Admins or SuperAdmins can create Managers
        if (!userIsAnAdmin && managerRoleRequested)
            return await Task.FromResult(
                OperationResult<RegisterUserResponseDto>.FailureResult(new[] { "Admin role required to assign manager roles" }, OperationErrorType.Forbidden));

        // Register the user (using admin registration logic for both roles)
        var result = await _accountService.RegisterAsync(request, request.Roles);
        
        if (!result.Success || result.Data is null)
            return await Task.FromResult(OperationResult<RegisterUserResponseDto>.FailureResult(result.Errors!));

        return await Task.FromResult(OperationResult<RegisterUserResponseDto>.SuccessResult(result.Data));
    }

    /// <summary>
        /// Authenticates a user via social login, allowing only Admin, Manager, or User roles, and registers the user if necessary without creating a customer.
        /// </summary>
        /// <param name="request">The social authentication request data.</param>
        /// <returns>An operation result containing the social authentication response.</returns>
        protected override async Task<OperationResult<SocialAuthResponseDto>> SocialLoginInternalAsync(SocialAuthRequestDto request) =>
        await _socialAuthService.AuthenticateAsync(
            request,
            new[] { RoleType.Admin, RoleType.Manager, RoleType.User },
            dto => _accountService.RegisterAsync(dto, dto.Roles, createCustomer: false, customerData: null)
        );
    
    /// <summary>
    /// Adds one or more roles to a specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user to whom roles will be added.</param>
    /// <param name="request">The request containing the roles to add.</param>
    /// <returns>HTTP 200 with updated role data on success; HTTP 400 with error messages on failure.</returns>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("users/{userId}/roles/add")]
    public async Task<IActionResult> AddRoles([FromRoute] string userId, [FromBody] AddRolesRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(request);
        
        var rolesString = string.Join(", ", request.Roles);
        _logger.Information("Admin role add request: {UserId}, Roles: {Role}", userId, rolesString);
        
        var result = await _rolesService.AddRoleAsync(userId, request);
        if (!result.Success)
        {
            _logger.Warning("Admin role add failed: {UserId}, Roles: {Role} - {Error}", userId, rolesString, result.Errors);
            return BadRequest(new { message = result.Errors });
        }
            
        _logger.Information("Admin role added successfully: {UserId}, Roles: {Role}", userId, rolesString);
        return Ok(result.Data);
    }

    /// <summary>
    /// Removes one or more roles from a specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose roles are to be removed.</param>
    /// <param name="request">The request containing the list of roles to remove.</param>
    /// <returns>HTTP 200 with updated role data on success; HTTP 400 with error messages on failure.</returns>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("users/{userId}/roles/remove")]
    public async Task<IActionResult> RemoveRoles([FromRoute] string userId, [FromBody] RemoveRolesRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(request);
        
        var roleList = string.Join(", ", request.Roles);
        _logger.Information("Admin role remove request: {UserId}, Role: {Role}", userId, nameof(roleList));
        
        var result = await _rolesService.RemoveRoleAsync(userId, request);
        if (!result.Success)
        {
            _logger.Warning("Admin role remove failed: {UserId}, Role: {Role} - {Error}", userId, nameof(roleList), result.Errors);
            return BadRequest(new { message = result.Errors });
        }
            
        _logger.Information("Admin role removed successfully: {UserId}, Role: {Role}", userId, nameof(roleList));
        return Ok(result.Data);
    }
    
    /// <summary>
    /// Retrieves the roles assigned to a specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose roles are to be retrieved.</param>
    /// <returns>HTTP 200 with the user's roles if successful; otherwise, HTTP 400 with error details.</returns>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpGet("users/{userId}/roles")]
    public async Task<IActionResult> GetRoles([FromRoute] string userId)
    {
        ArgumentNullException.ThrowIfNull(userId);
        
        var result = await _rolesService.GetRolesAsync(userId);
        if (!result.Success)
            return BadRequest(new { message = result.Errors });
         
        return Ok(result.Data);
    }

    /// <summary>
    /// Links a customer to a specified user by their IDs.
    /// </summary>
    /// <param name="linkRequest">The DTO containing the user ID and customer ID to link.</param>
    /// <returns>An HTTP 200 response with the result data if successful; otherwise, an HTTP 400 response with error details.</returns>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("users/link-customer")]
    public async Task<IActionResult> LinkCustomer(LinkCustomerDto linkRequest)
    {
        ArgumentNullException.ThrowIfNull(linkRequest);
        
        _logger.Information("Admin link customer request: {UserId}, CustomerId: {CustomerId}", 
            linkRequest.UserId, linkRequest.CustomerId);
            
        ArgumentNullException.ThrowIfNull(linkRequest.CustomerId);
        ArgumentNullException.ThrowIfNull(linkRequest.UserId);
            
        // Parse the user ID using the extension method
        var userIdResult = this.TryParseId(linkRequest.UserId, out UserId userId, _logger, UserId);
        if (userIdResult != null)
            return userIdResult;
            
        // Parse the customer ID using the extension method
        var customerIdResult = this.TryParseId(linkRequest.CustomerId, out CustomerId customerId, _logger, "CustomerId");
        if (customerIdResult != null)
            return customerIdResult;
                
        var result = await _customerService.LinkCustomerAsync(userId, customerId.ToString());
            
        if (!result.Success)
        {
            _logger.Warning("Admin link customer failed: {UserId}, {CustomerId} - {Error}",
                userId, customerId, result.Errors);
            return BadRequest(new { message = result.Errors });
        }
            
        _logger.Information("Customer linked successfully: {UserId}, {CustomerId}",
            userId, customerId);
        return Ok(result.Data);
    }
}
