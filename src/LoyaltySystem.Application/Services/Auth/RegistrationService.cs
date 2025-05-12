using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Customer;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Application.Interfaces.Auth;
using LoyaltySystem.Application.Validation;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace LoyaltySystem.Application.Services.Auth;

public class RegistrationService : IRegistrationService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICustomerRepository _customerRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IEmailVerificationRepository _emailVerificationRepository;
    private readonly ILogger _logger;
    
    public RegistrationService(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IUserRepository userRepository, 
        IPasswordHasher<User> passwordHasher,
        ICustomerRepository customerRepository,
        IEmailVerificationRepository emailVerificationRepository,
        ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        _emailVerificationRepository = emailVerificationRepository ?? throw new ArgumentNullException(nameof(emailVerificationRepository));
    }
    
    public async Task<OperationResult<UserDto>> RegisterUserAsync(RegisterUserDto registerDto, IEnumerable<RoleType> roles, bool createCustomer = false, CustomerExtraData? customerData = null)
    {
        var validator = new RegisterUserDtoValidator();
        var result = await validator.ValidateAsync(registerDto);
        
        if (!result.IsValid)
        {
            var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
            return OperationResult<UserDto>.FailureResult(errors);
        }
        
        var existingUserByEmail = await _userRepository.GetByEmailAsync(registerDto.Email);
        if (existingUserByEmail is { })
            return OperationResult<UserDto>.FailureResult($"Email '{registerDto.Email}' already exists.");

        var existingUserByUsername = await _userRepository.GetByUsernameAsync(registerDto.UserName);
        if (existingUserByUsername is { })
            return OperationResult<UserDto>.FailureResult($"Username '{registerDto.UserName}' already exists.");

        User? newUser = null;

        try
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                CustomerId customerId = null;
                if (createCustomer)
                {
                    // Create new Customer in the Database
                    var customer = new Domain.Entities.Customer
                    (
                        registerDto.FirstName,
                        registerDto.LastName,
                        registerDto.UserName,
                        registerDto.Email,
                        registerDto.Phone,
                        customerData?.Address,
                        customerData?.MarketingConsent ?? false,
                        null
                    );
                    var customerOutput = await _customerRepository.AddAsync(customer, _unitOfWork.CurrentTransaction);
                    customerId = customerOutput.Id ?? throw new InvalidOperationException("Failed to create customer or retrieve customer ID.");
                }
                
                newUser = new User 
                (
                    registerDto.FirstName,
                    registerDto.LastName,
                    registerDto.UserName,
                    registerDto.Email,
                    string.Empty,
                    customerId
                )
                {
                    IsEmailConfirmed = false
                };
                
                newUser.PasswordHash = _passwordHasher.HashPassword(newUser, registerDto.Password);

                foreach (var role in roles) 
                    newUser.AddRole(role);

                newUser.PrefixedId = newUser.Id.ToString();
                await _userRepository.AddAsync(newUser, _unitOfWork.CurrentTransaction);

                var token = SecurityUtils.GenerateSecureToken();
                var confirmationToken = new EmailConfirmationToken
                (
                    newUser.Id,
                    token,
                    DateTime.UtcNow.AddDays(1),
                    false,
                    null
                );
                await _emailVerificationRepository.SaveAsync(confirmationToken);
                await _emailService.SendEmailConfirmationEmailAsync(newUser.Email, token);
            });

            if (newUser is null)
            {
                _logger.Error("User object was null after registration transaction for email {Email}", registerDto.Email);
                return OperationResult<UserDto>.FailureResult("Registration completed but failed to retrieve user details.");
            }

            return OperationResult<UserDto>.SuccessResult(UserDto.From(newUser));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error during registration for email {Email}: {ErrorMessage}", registerDto.Email, ex.Message);
            return OperationResult<UserDto>.FailureResult($"Registration failed: {ex.Message}");
        }
    }
}
