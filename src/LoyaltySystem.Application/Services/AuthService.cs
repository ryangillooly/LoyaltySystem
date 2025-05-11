using FluentValidation;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
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
using System.IdentityModel.Tokens.Jwt;
using LoyaltySystem.Domain.Models;

namespace LoyaltySystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly ILogger _logger;

    public AuthService
    (
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        ICustomerRepository customerRepository,
        ILogger logger
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    }

    public async Task<OperationResult<AuthResponseDto>> AuthenticateAsync(string identifier, string password, LoginIdentifierType identifierType)
    {
        User? user;
            
        switch (identifierType)
        {
            case LoginIdentifierType.Email:
                user = await _userRepository.GetByEmailAsync(identifier);
                break;
                    
            case LoginIdentifierType.Username:
                user = await _userRepository.GetByUsernameAsync(identifier);
                break;
                    
            default:
                return OperationResult<AuthResponseDto>.FailureResult("Invalid identifier type");
        }
            
        if (user is null)
            return OperationResult<AuthResponseDto>.FailureResult("Invalid username/email or password");
                
        if (user.Status != UserStatus.Active)
            return OperationResult<AuthResponseDto>.FailureResult("User account is not active");
                
        if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            return OperationResult<AuthResponseDto>.FailureResult("Invalid username/email or password");
            
        user.RecordLogin();
        await _userRepository.UpdateAsync(user);

        // Generate JWT token result (includes expiry, etc.)
        var tokenResult = GenerateTokenResult(user);
        
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
    
    public async Task<OperationResult<AuthResponseDto>> AuthenticateForAppAsync(
        string identifier, 
        string password, 
        LoginIdentifierType identifierType,
        IEnumerable<RoleType> allowedRoles)
    {
        // First authenticate the user
        return await AuthenticateAsync(identifier, password, identifierType);
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

                CreatePasswordHash(registerDto.Password, out string passwordHash, out string passwordSalt);

                newUser = new User 
                (
                    registerDto.FirstName,
                    registerDto.LastName,
                    registerDto.UserName,
                    registerDto.Email,
                    passwordHash,
                    passwordSalt,
                    customerId
                );

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

    #region Helper Methods

    private TokenResult GenerateTokenResult(User user)
    {
        var claims = new List<Claim>();
        
        if (user.Id is { })
            claims.Add(new Claim("UserId", user.Id.ToString()));
        
        if (!string.IsNullOrEmpty(user.UserName))
            claims.Add(new Claim("Username", user.UserName));
        
        if (!string.IsNullOrEmpty(user.Email))
            claims.Add(new Claim("Email", user.Email));
        
        if (user.Status is { })
            claims.Add(new Claim("Status", user.Status.ToString()));
        
        if (user.CustomerId is { })
            claims.Add(new Claim("CustomerId", user.CustomerId.ToString())); // Add prefixed customer ID

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role.Role.ToString()))); // Access Role property of UserRole
        
        // Call the service to get the TokenResult
        return _jwtService.GenerateToken(claims); 
    }

    private static bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(storedSalt))
            return false;
        using var hmac = new System.Security.Cryptography.HMACSHA512(Convert.FromBase64String(storedSalt));
        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return computedHash.SequenceEqual(Convert.FromBase64String(storedHash));
    }

    private static void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        using var hmac = new System.Security.Cryptography.HMACSHA512();
        passwordSalt = Convert.ToBase64String(hmac.Key);
        passwordHash = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));
    }

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