using FluentValidation;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.PasswordReset;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Application.DTOs.Customer;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Application.Validation;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Domain.Interfaces;
using Serilog;
using System.Security.Claims;
using LoyaltySystem.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;

namespace LoyaltySystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public AuthService
    (
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        ICustomerRepository customerRepository,
        ITokenService tokenService,
        IEmailService emailService,
        IPasswordHasher<User> passwordHasher,
        ILogger logger
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    }

    public async Task<OperationResult<AuthResponseDto>> AuthenticateAsync(LoginRequestDto dto)
    {
        User? user = await GetUserByEmailOrUsernameAsync(dto);
            
        if (user is null)
            return OperationResult<AuthResponseDto>.FailureResult("Invalid username/email or password");
                
        if (user.Status != UserStatus.Active)
            return OperationResult<AuthResponseDto>.FailureResult("User account is not active");

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
            return OperationResult<AuthResponseDto>.FailureResult("Invalid username/email or password");
            
        user.RecordLogin();
        await _userRepository.UpdateAsync(user);

        // Generate JWT token result (includes expiry, etc.)
        var tokenResult = _jwtService.GenerateTokenResult(user);
        
        // Prepare response DTO from TokenResult
        var response = new AuthResponseDto
        {
            AccessToken = tokenResult.AccessToken,
            TokenType = tokenResult.TokenType, 
            ExpiresIn = tokenResult.ExpiresIn
            // RefreshToken = tokenResult.RefreshToken // Assign if/when implemented
        };
        
        return OperationResult<AuthResponseDto>.SuccessResult(response);
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
                    var customer = new Customer
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
                );
                
                newUser.PasswordHash = _passwordHasher.HashPassword(newUser, registerDto.Password);

                foreach (var role in roles) 
                    newUser.AddRole(role);

                newUser.PrefixedId = newUser.Id.ToString();
                await _userRepository.AddAsync(newUser, _unitOfWork.CurrentTransaction);
            });

            if (newUser is null)
            {
                _logger.Error("User object was null after registration transaction for email {Email}", registerDto.Email);
                return OperationResult<UserDto>.FailureResult("Registration completed but failed to retrieve user details.");
            }

            return OperationResult<UserDto>.SuccessResult(MapUserToDto(newUser));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error during registration for email {Email}: {ErrorMessage}", registerDto.Email, ex.Message);
            return OperationResult<UserDto>.FailureResult($"Registration failed: {ex.Message}");
        }
    }
    
    public async Task<OperationResult<UserDto>> AddCustomerRoleToUserAsync(string userId)
    {
        UserId userIdObj;
        try
        {
            userIdObj = UserId.FromString(userId);
        }
        catch (FormatException ex)
        {
            return OperationResult<UserDto>.FailureResult($"Invalid UserId format: {ex.Message}");
        }

        var user = await _userRepository.GetByIdAsync(userIdObj);
        if (user is null)
            return OperationResult<UserDto>.FailureResult("User not found.");

        if (user.CustomerId is null)
            return OperationResult<UserDto>.FailureResult("User is not linked to a customer record.");

        await _userRepository.AddRoleAsync(userIdObj, RoleType.Customer);
        // Re-fetch user to get updated roles for the DTO
        var updatedUser = await _userRepository.GetByIdAsync(userIdObj);
        return OperationResult<UserDto>.SuccessResult(MapUserToDto(updatedUser!));
    }
    
    public async Task<OperationResult<UserDto>> UpdateProfileAsync(string userId, UpdateProfileDto updateDto)
    {
        UserId userIdObj;
        try
        {
            userIdObj = UserId.FromString(userId);
        }
        catch (FormatException ex)
        {
            return OperationResult<UserDto>.FailureResult($"Invalid UserId format: {ex.Message}");
        }
        
        var user = await _userRepository.GetByIdAsync(userIdObj);
        if (user is null)
            return OperationResult<UserDto>.FailureResult("User not found.");

        // Basic validation
        if (string.IsNullOrEmpty(updateDto.UserName) && string.IsNullOrEmpty(updateDto.Email))
            return OperationResult<UserDto>.FailureResult("No profile information provided for update.");

        // Update fields if provided
        if (!string.IsNullOrEmpty(updateDto.UserName))
            user.UpdateUserName(updateDto.UserName);
            
        if (!string.IsNullOrEmpty(updateDto.Email))
            user.UpdateEmail(updateDto.Email);
            
        // Persist changes (assuming UpdateAsync takes User object)
        await _userRepository.UpdateAsync(user); 

        return OperationResult<UserDto>.SuccessResult(MapUserToDto(user));
    }
    public async Task<OperationResult<UserProfileDto>> GetUserByIdAsync(string userId)
    {
        UserId userIdObj;
        try
        {
            userIdObj = UserId.FromString(userId);
        } 
        catch (FormatException ex)
        {
            _logger.Error(ex, "Invalid format for UserId: {UserId}", userId);
            return OperationResult<UserProfileDto>.FailureResult($"Invalid UserId format: {ex.Message}");
        }
            
        var user = await _userRepository.GetByIdAsync(userIdObj);
        if (user is null)
        {
            _logger.Warning("User not found for UserId: {UserId}", userId);
            return OperationResult<UserProfileDto>.FailureResult("User not found.");
        }

        // Attempt to fetch associated Customer data
        Customer? customer = null;
        if (user.CustomerId != null)
        {
            try
            {
                customer = await _customerRepository.GetByIdAsync(user.CustomerId);
                if (customer is null)
                {
                     // Log this situation - user has a CustomerId but customer record is missing?
                     _logger.Warning("User {UserId} has CustomerId {CustomerId}, but the customer record was not found.", user.PrefixedId, user.CustomerId.ToString());
                }
            }
            catch (Exception ex)
            {
                 // Log error fetching customer
                 _logger.Error(ex, "Error fetching customer details for CustomerId {CustomerId} linked to User {UserId}", user.CustomerId.ToString(), user.PrefixedId);
                 // Continue without customer data, or return failure? Depends on requirements.
                 // For now, we'll continue and return profile with potentially missing name.
            }
        }
            
        // *** Add Logging Here ***
        if (customer != null)
        {
            _logger.Information("Fetched customer for profile: Id={CustomerId}, PrefixedId={PrefixedId}, FirstName='{FirstName}', LastName='{LastName}'", 
                customer.Id.Value, customer.PrefixedId, customer.FirstName, customer.LastName);
        }
        else
        {
            _logger.Warning("Customer object was null when mapping profile for User {UserId}", user.PrefixedId);
        }
        // *** End Logging ***

        // Map data from User and potentially Customer to UserProfileDto
        var profileDto = new UserProfileDto
        {
            Id = user.Id.ToString(), // Use the prefixed string ID
            FirstName = customer?.FirstName ?? user.FirstName, // Prioritize Customer.FirstName, fallback to User.FirstName (which might be empty)
            LastName = customer?.LastName ?? user.LastName,   // Prioritize Customer.LastName, fallback to User.LastName
            UserName = user.UserName,
            Email = user.Email,
            Status = user.Status.ToString(),
            CustomerId = user.CustomerId?.ToString(), // Already a prefixed string from EntityId.ToString()
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = user.Roles.Select(ur => ur.Role.ToString()).ToList() // Correctly map Role enum
        };
            
        return OperationResult<UserProfileDto>.SuccessResult(profileDto);
    }
    public async Task<OperationResult<UserProfileDto>> GetUserByEmailAsync(string userEmail)
    {
        var user = await _userRepository.GetByEmailAsync(userEmail);
        if (user is null)
        {
            _logger.Warning("User not found for Email: {Email}", userEmail);
            return OperationResult<UserProfileDto>.FailureResult($"User not found with email {userEmail}");
        }

        // Attempt to fetch associated Customer data
        Customer? customer = null;
        if (user.CustomerId is { })
        {
            try
            {
                customer = await _customerRepository.GetByIdAsync(user.CustomerId);
                if (customer is null)
                    _logger.Warning("User {UserId} has CustomerId {CustomerId}, but the customer record was not found.", user.PrefixedId, user.CustomerId.ToString());
            }
            catch (Exception ex)
            {
                 // Log error fetching customer
                 _logger.Error(ex, "Error fetching customer details for CustomerId {CustomerId} linked to User {UserId}", user.CustomerId.ToString(), user.PrefixedId);
                 // Continue without customer data, or return failure? Depends on requirements.
                 // For now, we'll continue and return profile with potentially missing name.
            }
        }
        
        if (customer is { })
            _logger.Information("Fetched customer for profile: Id={CustomerId}, PrefixedId={PrefixedId}, FirstName='{FirstName}', LastName='{LastName}'", 
                customer.Id.Value, customer.PrefixedId, customer.FirstName, customer.LastName);
        else
            _logger.Warning("Customer object was null when mapping profile for User {UserId}", user.PrefixedId);

        // Map data from User and potentially Customer to UserProfileDto
        var profileDto = new UserProfileDto
        {
            Id = user.Id.ToString(), // Use the prefixed string ID
            FirstName = customer?.FirstName ?? user.FirstName,
            LastName = customer?.LastName ?? user.LastName,   
            UserName = user.UserName,
            Email = user.Email,
            Status = user.Status.ToString(),
            CustomerId = user.CustomerId?.ToString(), 
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = user.Roles.Select(ur => ur.Role.ToString()).ToList() // Correctly map Role enum
        };
            
        return OperationResult<UserProfileDto>.SuccessResult(profileDto);
    }
    public Task<OperationResult<UserProfileDto>> GetUserByUsernameAsync(string username) =>
        throw new NotImplementedException();

    public async Task<OperationResult<UserDto>> GetUserByCustomerIdAsync(string customerId)
    {
        CustomerId customerIdObj;
        try
        {
            customerIdObj = CustomerId.FromString(customerId);
        }
        catch (FormatException ex)
        {
            return OperationResult<UserDto>.FailureResult($"Invalid CustomerId format: {ex.Message}");
        }
            
        var user = await _userRepository.GetByCustomerIdAsync(customerIdObj);
        
        // Temporarily mapping to UserDto until MapUserToDto is removed or refactored
        Func<User, UserDto> tempMapper = u => new UserDto
        {
            Id = u.Id.Value,
            PrefixedId = u.PrefixedId ?? string.Empty, // Fallback
            FirstName = u.FirstName ?? string.Empty, // Fallback
            LastName = u.LastName ?? string.Empty,   // Fallback
            UserName = u.UserName ?? string.Empty, // Fallback
            Email = u.Email ?? string.Empty, // Fallback
            Status = u.Status.ToString(),
            CustomerId = u.CustomerId?.ToString(), // Already handles null
            CreatedAt = u.CreatedAt,
            LastLoginAt = u.LastLoginAt,
            Roles = u.Roles.Select(r => r.Role.ToString()).ToList() // Role should not be null
        };

        return user is null
            ? OperationResult<UserDto>.FailureResult("User not found for the given Customer ID.")
            : OperationResult<UserDto>.SuccessResult(tempMapper(user)); // Use temp mapper
    }
    public async Task<OperationResult<UserDto>> AddRoleAsync(string userId, RoleType role)
    {
        UserId userIdObj;
        try
        {
            userIdObj = UserId.FromString(userId);
        }
        catch (FormatException ex)
        {
            return OperationResult<UserDto>.FailureResult($"Invalid UserId format: {ex.Message}");
        }

        var user = await _userRepository.GetByIdAsync(userIdObj);
        if (user is null)
            return OperationResult<UserDto>.FailureResult("User not found.");
            
        await _userRepository.AddRoleAsync(userIdObj, role);
        // Re-fetch user to get updated roles for the DTO
        var updatedUser = await _userRepository.GetByIdAsync(userIdObj);
        
        // Temporarily mapping to UserDto
        Func<User, UserDto> tempMapper = u => new UserDto 
        {
            Id = u.Id.Value, 
            PrefixedId = u.PrefixedId ?? string.Empty, // Fallback
            FirstName = u.FirstName ?? string.Empty, // Fallback
            LastName = u.LastName ?? string.Empty, // Fallback
            UserName = u.UserName ?? string.Empty, // Fallback
            Email = u.Email ?? string.Empty, // Fallback
            Status = u.Status.ToString(), 
            CustomerId = u.CustomerId?.ToString(), // Already handles null
            CreatedAt = u.CreatedAt, 
            LastLoginAt = u.LastLoginAt, 
            Roles = u.Roles.Select(r => r.Role.ToString()).ToList() // Role should not be null
        };
        
        return OperationResult<UserDto>.SuccessResult(tempMapper(updatedUser!)); 
    }
    public async Task<OperationResult<UserDto>> RemoveRoleAsync(string userId, RoleType role)
    {
        UserId userIdObj;
        try
        {
            userIdObj = UserId.FromString(userId);
        }
        catch (FormatException ex)
        {
            return OperationResult<UserDto>.FailureResult($"Invalid UserId format: {ex.Message}");
        }

        var user = await _userRepository.GetByIdAsync(userIdObj);
        if (user is null)
            return OperationResult<UserDto>.FailureResult("User not found.");
            
        await _userRepository.RemoveRoleAsync(userIdObj, role);
        // Re-fetch user to get updated roles for the DTO
        var updatedUser = await _userRepository.GetByIdAsync(userIdObj);
        
        // Temporarily mapping to UserDto
        Func<User, UserDto> tempMapper = u => new UserDto 
        {
            Id = u.Id.Value, 
            PrefixedId = u.PrefixedId, 
            FirstName = u.FirstName, 
            LastName = u.LastName,
            UserName = u.UserName, 
            Email = u.Email, 
            Status = u.Status.ToString(), 
            CustomerId = u.CustomerId?.ToString(),
            CreatedAt = u.CreatedAt, 
            LastLoginAt = u.LastLoginAt, 
            Roles = u.Roles.Select(r => r.Role.ToString()).ToList()
        };

        return OperationResult<UserDto>.SuccessResult(tempMapper(updatedUser!)); 
    }
    public async Task<OperationResult<UserDto>> LinkCustomerAsync(string userId, string customerId)
    {
        UserId userIdObj;
        CustomerId customerIdObj;
        try
        {
            userIdObj = UserId.FromString(userId);
            customerIdObj = CustomerId.FromString(customerId);
        }
        catch (FormatException ex)
        {
            return OperationResult<UserDto>.FailureResult($"Invalid ID format: {ex.Message}");
        }

        var user = await _userRepository.GetByIdAsync(userIdObj);
        if (user == null)
            return OperationResult<UserDto>.FailureResult("User not found.");

        var customer = await _customerRepository.GetByIdAsync(customerIdObj);
        if (customer == null)
            return OperationResult<UserDto>.FailureResult("Customer not found.");

        if (user.CustomerId != null)
            return OperationResult<UserDto>.FailureResult("User is already linked to a customer.");

        // Directly set the CustomerId property
        user.CustomerId = customerIdObj; 
        user.AddRole(RoleType.Customer); // Also ensure the Customer role is added
        await _userRepository.UpdateAsync(user);
        
        // Temporarily mapping to UserDto
        Func<User, UserDto> tempMapper = u => new UserDto 
        {
            Id = u.Id.Value, PrefixedId = u.PrefixedId, FirstName = u.FirstName, LastName = u.LastName,
            UserName = u.UserName, Email = u.Email, Status = u.Status.ToString(), CustomerId = u.CustomerId?.ToString(),
            CreatedAt = u.CreatedAt, LastLoginAt = u.LastLoginAt, Roles = u.Roles.Select(r => r.ToString()).ToList()
        };

        return OperationResult<UserDto>.SuccessResult(tempMapper(user));
    }

    
    public async Task<OperationResult> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        User? user = await GetUserByEmailOrUsernameAsync(request);

        if (user is null)
            return OperationResult.FailureResult($"User not found with {request.IdentifierType} == {request.Identifier}");
        
        var token = await _tokenService.GeneratePasswordResetTokenAsync(user);
        await _emailService.SendPasswordResetEmailAsync(user.Email, token);

        return OperationResult.SuccessResult();
    }

    public async Task<OperationResult> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        User? user = await _userRepository.GetByEmailAsync(request.Email);
        
        if (user is null)
            return OperationResult.FailureResult($"User not found with the Email == {request.Email}");

        var isValid = await _tokenService.ValidatePasswordResetTokenAsync(user, request.Token);
        if (!isValid)
            return OperationResult.FailureResult("Invalid or expired reset token.");

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        await _userRepository.UpdateAsync(user);

        await _tokenService.InvalidatePasswordResetTokenAsync(user, request.Token);
        
        return OperationResult.SuccessResult();
    }
    
    #region Helper Methods

    private async Task<User?> GetUserByEmailOrUsernameAsync(AuthDto request) =>
        request.IdentifierType switch
        {
            AuthIdentifierType.Email    => await _userRepository.GetByEmailAsync(request.Identifier),
            AuthIdentifierType.Username => await _userRepository.GetByUsernameAsync(request.Identifier),
                                      _ => null
        };
    
    private static UserDto MapUserToDto(User user) =>
        new ()
        {
            Id = user.Id.Value,
            PrefixedId = user.Id.ToString(),
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.UserName,
            Email = user.Email,
            Status = user.Status.ToString(),
            CustomerId = user.CustomerId?.ToString() ?? string.Empty,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = user.Roles.Select(r => r.Role.ToString()).ToList()
        };

    #endregion
}