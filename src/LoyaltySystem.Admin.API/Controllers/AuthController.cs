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
    
    [HttpPost("register")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public override async Task<IActionResult> Register(RegisterUserRequestDto request) => await base.Register(request);
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

    protected override async Task<OperationResult<SocialAuthResponseDto>> SocialLoginInternalAsync(SocialAuthRequestDto request) =>
        await _socialAuthService.AuthenticateAsync(
            request,
            new[] { RoleType.Admin, RoleType.Manager, RoleType.User },
            dto => _accountService.RegisterAsync(dto, dto.Roles, createCustomer: false, customerData: null)
        );
    
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("users/{userId}/roles/add")]
    public async Task<IActionResult> AddRole([FromRoute] string userId, [FromBody] AddRolesRequestDto request)
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

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("users/{userId}/roles/remove")]
    public async Task<IActionResult> RemoveRole([FromRoute] string userId, [FromBody] RemoveRolesRequestDto request)
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
    
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpGet("users/{userId}/roles")]
    public async Task<IActionResult> RemoveRole([FromRoute] string userId)
    {
        ArgumentNullException.ThrowIfNull(userId);
        
        var result = await _rolesService.GetRolesAsync(userId);
        if (!result.Success)
            return BadRequest(new { message = result.Errors });
         
        return Ok(result.Data);
    }

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
