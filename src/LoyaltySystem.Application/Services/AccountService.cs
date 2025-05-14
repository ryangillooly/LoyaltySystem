using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.PasswordReset;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Application.DTOs.Customers;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Application.Interfaces.Auth;
using LoyaltySystem.Application.Interfaces.Customers;
using LoyaltySystem.Application.Interfaces.Users;
using LoyaltySystem.Application.Validation;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace LoyaltySystem.Application.Services;

public class AccountService : IAccountService 
{
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher<InternalUserDto> _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICustomerService _customerService;
    private readonly ILogger _logger;

    public AccountService(
        ITokenService tokenService,
        IEmailService emailService,
        IUserService userService,
        ICustomerService customerService,
        IPasswordHasher<InternalUserDto> passwordHasher,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _logger = logger;
        _userService = userService;
        _tokenService = tokenService;
        _emailService = emailService;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _customerService = customerService;
    }
    
    public async Task<OperationResult> VerifyEmailAsync(string token)
    {
        ArgumentNullException.ThrowIfNull(token);
        
        var record = await _tokenService.GetValidTokenAsync(VerificationTokenType.EmailVerification, token);
        if (record is not { IsValid: true } || record.ExpiresAt < DateTime.UtcNow)
            return OperationResult.FailureResult("Invalid or expired verification token.");
        
        var result = await _userService.ConfirmEmailAndUpdateAsync(record.UserId);
        if (!result.Success)
            return result;
        
        await _tokenService.InvalidateAllTokensAsync(record.UserId, VerificationTokenType.EmailVerification);
        return OperationResult.SuccessResult();
    }
    public async Task<OperationResult> ResendVerificationEmailAsync(string email)
    {
        ArgumentNullException.ThrowIfNull(email);
        
        var user = await _userService.GetByEmailAsync(email);
        if (user?.Data is null)
            return OperationResult.FailureResult("User not found");
        
        await _tokenService.InvalidateAllTokensAsync(user.Data.Id, VerificationTokenType.EmailVerification);
        
        var token = await _tokenService.GenerateAndStoreTokenAsync(user.Data.Id, VerificationTokenType.EmailVerification, TimeSpan.FromDays(1));
        await _emailService.SendConfirmationEmailAsync(email, token);

        return OperationResult.SuccessResult();
    }
    
    public async Task<OperationResult> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var validator = new ForgotPasswordRequestDtoValidator();
        var result = await validator.ValidateAsync(request);
        if (!result.IsValid)
        {
            var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
            return OperationResult.FailureResult(errors);
        }
        
        var user = await GetUserByEmailOrUsernameAsync(request);
        if (!user.Success)
        {
            _logger.Information("User not found. {Type} == {Identifier}", request.IdentifierType, request.Identifier);
            return OperationResult.FailureResult($"User not found with {request.IdentifierType} == {request.Identifier}");
        }

        var token = await _tokenService.GenerateAndStoreTokenAsync(user.Data.Id, VerificationTokenType.PasswordReset, TimeSpan.FromMinutes(30));
        await _emailService.SendPasswordResetEmailAsync(user.Data.Email, token);

        _logger.Information("Password successfully reset for {Type} == {Identifier}", request.IdentifierType, request.Identifier);
        return OperationResult.SuccessResult();
    }
    
    public async Task<OperationResult> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var validator = new ResetPasswordRequestDtoValidator();
        var result = await validator.ValidateAsync(request);
        if (!result.IsValid)
        {
            var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
            return OperationResult.FailureResult(errors);
        }
        
        var userResult = await GetUserByEmailOrUsernameAsync(request);
        if (!userResult.Success || userResult.Data is null)
        {
            _logger.Information("User not found. {Email}", request.Email);
            return OperationResult.FailureResult($"User not found with the Email == {request.Email}");
        }

        var user = userResult.Data;
        
        var tokenResult = await _tokenService.IsTokenValidAsync(VerificationTokenType.PasswordReset, request.Token);
        if (!tokenResult.Success)
        {
            _logger.Information("Password reset token is invalid. {Token}. For Email {Email}", request.Token, request.Email);
            return OperationResult.FailureResult("Invalid or expired reset token.");
        }
        
        var updateResult = await _userService.UpdatePasswordAsync(user, request);
        if (!updateResult.Success)
            return OperationResult.FailureResult(updateResult.Errors);
        
        await _tokenService.InvalidateAllTokensAsync(user.Id, VerificationTokenType.PasswordReset);
        
        _logger.Information("Successfully reset password For Email {Email}", request.Email);
        return OperationResult.SuccessResult();
    }
    
    public async Task<OperationResult<InternalUserDto>> RegisterAsync(RegisterUserDto registerDto, IEnumerable<RoleType> roles, bool createCustomer = false, CustomerExtraData? customerData = null)
    {
        var validator = new RegisterUserDtoValidator();
        var validationResult = await validator.ValidateAsync(registerDto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return OperationResult<InternalUserDto>.FailureResult(errors);
        }
        
        var getByEmailResult = await _userService.GetByEmailAsync(registerDto.Email);
        if (getByEmailResult.Success)
            return OperationResult<InternalUserDto>.FailureResult($"Email '{registerDto.Email}' already exists.");

        var getByUsernameResult = await _userService.GetByUsernameAsync(registerDto.UserName);
        if (getByUsernameResult.Success)
            return OperationResult<InternalUserDto>.FailureResult($"Username '{registerDto.UserName}' already exists.");

        var user = BuildUserObject(registerDto, roles);
        var customer = BuildCreateCustomerDto(registerDto, createCustomer, customerData);

        try
        {
            // TODO: Come back and check this, may need to return customer object 
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (customer is { })
                    await _customerService.AddCustomerAsync(customer, _unitOfWork.CurrentTransaction); 

                await _userService.AddAsync(user, _unitOfWork.CurrentTransaction);
                var token = await _tokenService.GenerateAndStoreTokenAsync(user.Id, VerificationTokenType.EmailVerification, TimeSpan.FromDays(1));
                await _emailService.SendConfirmationEmailAsync(user.Email, token);
            });

            if (user is null)
            {
                _logger.Error("User object was null after registration transaction for email {Email}", registerDto.Email);
                return OperationResult<InternalUserDto>.FailureResult("Registration completed but failed to retrieve user details.");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error during registration for email {Email}: {ErrorMessage}", registerDto.Email, ex.Message);
            return OperationResult<InternalUserDto>.FailureResult($"Registration failed: {ex.Message}");
        }
        
        return OperationResult<InternalUserDto>.SuccessResult(InternalUserDto.From(user));
    }
    
    
    # region Helpers
    private static CreateCustomerDto? BuildCreateCustomerDto(RegisterUserDto dto, bool createCustomer, CustomerExtraData? customerData) =>
        createCustomer
            ? new CreateCustomerDto
              {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                UserName = dto.UserName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = customerData?.Address,
                MarketingConsent = customerData?.MarketingConsent ?? false
              }
            : null;
    
    private User BuildUserObject(RegisterUserDto registerDto, IEnumerable<RoleType> roles)
    {
        User user =  new User 
        (
            registerDto.FirstName,
            registerDto.LastName,
            registerDto.UserName,
            registerDto.Email,
            string.Empty,
            registerDto.Phone,
            null
        )
        {
            IsEmailConfirmed = false
        };
                
        user.PasswordHash = _passwordHasher.HashPassword(InternalUserDto.From(user), registerDto.Password);

        foreach (var role in roles) 
            user.AddRole(role);

        user.PrefixedId = user.Id.ToString();
        return user;
    }
    private async Task<OperationResult<InternalUserDto>> GetUserByEmailOrUsernameAsync(AuthDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.IdentifierType);
        
        return request.IdentifierType switch
        {
            AuthIdentifierType.Email    => await _userService.GetByEmailAsync(request.Identifier),
            AuthIdentifierType.Username => await _userService.GetByUsernameAsync(request.Identifier),
            _ => OperationResult<InternalUserDto>.FailureResult("Invalid/Unknown identifier")
        };
    }
    # endregion
}
