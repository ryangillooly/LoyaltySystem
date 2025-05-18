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

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountService"/> class with required services for account management operations.
    /// </summary>
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
    
    /// <summary>
    /// Verifies a user's email address using a verification token.
    /// </summary>
    /// <param name="token">The email verification token to validate.</param>
    /// <returns>An <see cref="OperationResult"/> indicating success or failure of the email verification process.</returns>
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
    /// <summary>
    /// Sends a new email verification link to the specified email address, invalidating any previous verification tokens.
    /// </summary>
    /// <param name="email">The email address to which the verification email will be sent.</param>
    /// <returns>An <see cref="OperationResult"/> indicating success or failure.</returns>
    public async Task<OperationResult> ResendVerificationEmailAsync(string email)
    {
        ArgumentNullException.ThrowIfNull(email);
        
        var user = await _userService.GetByEmailAsync(email);
        if (user?.Data is null)
            return OperationResult.FailureResult("User not found");
        
        await _tokenService.InvalidateAllTokensAsync(user.Data.Id, VerificationTokenType.EmailVerification);
        
        var token = await _tokenService.GenerateAndStoreTokenAsync(user.Data.Id, VerificationTokenType.EmailVerification);
        await _emailService.SendConfirmationEmailAsync(email, token);

        return OperationResult.SuccessResult();
    }
    
    /// <summary>
    /// Initiates the password reset process by validating the request, generating a password reset token, and sending a reset email to the user.
    /// </summary>
    /// <param name="request">The password reset request containing the user's identifier.</param>
    /// <returns>An <see cref="OperationResult"/> indicating the success or failure of the operation.</returns>
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

        var token = await _tokenService.GenerateAndStoreTokenAsync(user.Data.Id, VerificationTokenType.PasswordReset);
        await _emailService.SendPasswordResetEmailAsync(user.Data.Email, token);

        _logger.Information("Password successfully reset for {Type} == {Identifier}", request.IdentifierType, request.Identifier);
        return OperationResult.SuccessResult();
    }
    
    /// <summary>
    /// Resets a user's password using a valid password reset token.
    /// </summary>
    /// <param name="request">The password reset request containing user identifier, new password, and reset token.</param>
    /// <returns>An <see cref="OperationResult"/> indicating success or failure, with error messages if applicable.</returns>
    public async Task<OperationResult> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        
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
    
    /// <summary>
    /// Registers a new user with the specified roles, optionally creating an associated customer record, and sends an email verification link.
    /// </summary>
    /// <param name="registerRequestDto">The registration details for the new user.</param>
    /// <param name="roles">The roles to assign to the user.</param>
    /// <param name="createCustomer">Whether to create a customer record linked to the user.</param>
    /// <param name="customerData">Optional extra data for customer creation.</param>
    /// <returns>An <see cref="OperationResult{RegisterUserResponseDto}"/> indicating success or failure, with user registration details on success.</returns>
    public async Task<OperationResult<RegisterUserResponseDto>> RegisterAsync(RegisterUserRequestDto registerRequestDto, IEnumerable<RoleType> roles, bool createCustomer = false, CustomerExtraData? customerData = null)
    {
        var validator = new RegisterUserDtoValidator();
        var validationResult = await validator.ValidateAsync(registerRequestDto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return OperationResult<RegisterUserResponseDto>.FailureResult(errors);
        }
        
        var getByEmailResult = await _userService.GetByEmailAsync(registerRequestDto.Email);
        if (getByEmailResult.Success)
            return OperationResult<RegisterUserResponseDto>.FailureResult($"Email '{registerRequestDto.Email}' already exists.");

        var getByUsernameResult = await _userService.GetByUsernameAsync(registerRequestDto.Username);
        if (getByUsernameResult.Success)
            return OperationResult<RegisterUserResponseDto>.FailureResult($"Username '{registerRequestDto.Username}' already exists.");
        
        var getByPhoneResult = await _userService.GetByPhoneNumberAsync(registerRequestDto.Username);
        if (getByPhoneResult.Success)
            return OperationResult<RegisterUserResponseDto>.FailureResult($"Phone Number '{registerRequestDto.Phone}' already exists.");

        var user = BuildUserObject(registerRequestDto, roles);
        var customer = BuildCreateCustomerDto(registerRequestDto, createCustomer, customerData);

        try
        {
            // TODO: Come back and check this, may need to return customer object 
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (customer is { })
                    await _customerService.AddCustomerAsync(customer, _unitOfWork.CurrentTransaction); 

                await _userService.AddAsync(user, _unitOfWork.CurrentTransaction);
                var token = await _tokenService.GenerateAndStoreTokenAsync(user.Id, VerificationTokenType.EmailVerification);
                await _emailService.SendConfirmationEmailAsync(user.Email, token);
            });

            if (user is null)
            {
                _logger.Error("User object was null after registration transaction for email {Email}", registerRequestDto.Email);
                return OperationResult<RegisterUserResponseDto>.FailureResult("Registration completed but failed to retrieve user details.");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error during registration for email {Email}: {ErrorMessage}", registerRequestDto.Email, ex.Message);
            return OperationResult<RegisterUserResponseDto>.FailureResult($"Registration failed: {ex.Message}");
        }
        
        return OperationResult<RegisterUserResponseDto>.SuccessResult(RegisterUserResponseDto.From(user));
    }
    
    
    # region Helpers
    /// <summary>
            /// Constructs a <see cref="CreateCustomerDto"/> from registration and optional customer data if customer creation is requested; otherwise returns null.
            /// </summary>
            /// <param name="requestDto">The registration request containing user details.</param>
            /// <param name="createCustomer">Indicates whether to create a customer record.</param>
            /// <param name="customerData">Optional additional customer data.</param>
            /// <returns>A populated <see cref="CreateCustomerDto"/> if <paramref name="createCustomer"/> is true; otherwise, null.</returns>
            private static CreateCustomerDto? BuildCreateCustomerDto(RegisterUserRequestDto requestDto, bool createCustomer, CustomerExtraData? customerData) =>
        createCustomer
            ? new CreateCustomerDto
              {
                FirstName = requestDto.FirstName,
                LastName = requestDto.LastName,
                Username = requestDto.Username,
                Email = requestDto.Email,
                Phone = requestDto.Phone,
                Address = customerData?.Address,
                MarketingConsent = customerData?.MarketingConsent ?? false
              }
            : null;
    
    /// <summary>
    /// Constructs a new User entity from registration data and assigns specified roles with a hashed password.
    /// </summary>
    /// <param name="registerRequestDto">The registration data for the new user.</param>
    /// <param name="roles">The roles to assign to the user.</param>
    /// <returns>A User entity populated with the provided data and roles.</returns>
    private User BuildUserObject(RegisterUserRequestDto registerRequestDto, IEnumerable<RoleType> roles)
    {
        User user =  new User 
        (
            registerRequestDto.FirstName,
            registerRequestDto.LastName,
            registerRequestDto.Username,
            registerRequestDto.Email,
            string.Empty,
            registerRequestDto.Phone,
            null
        )
        {
            IsEmailConfirmed = false
        };
                
        user.PasswordHash = _passwordHasher.HashPassword(InternalUserDto.From(user), registerRequestDto.Password);

        foreach (var role in roles) 
            user.AddRole(role);

        return user;
    }
    /// <summary>
    /// Retrieves a user by email or username based on the identifier type specified in the authentication request.
    /// </summary>
    /// <param name="request">Authentication data containing the identifier and its type.</param>
    /// <returns>An operation result containing the user if found, or a failure result if not found or if the identifier type is invalid.</returns>
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
