using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
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
    private readonly ICustomerService _customerService;
    public AuthController(IAuthService authService, ICustomerService customerService, ILogger logger)
        : base(authService, logger) => 
            _customerService = customerService;

    protected override string UserType => "Customer";
    protected override async Task<OperationResult<UserDto>> RegisterAsync(RegisterUserDto registerRequest) => 
        await _authService.RegisterCustomerAsync(registerRequest);

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