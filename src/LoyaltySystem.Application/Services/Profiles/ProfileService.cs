using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Application.Interfaces.Profile;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using Serilog;

namespace LoyaltySystem.Application.Services.Profiles;

public class ProfileService : IProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger _logger;

    public ProfileService(
        IUserRepository userRepository, 
        ICustomerRepository customerRepository,
        ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
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

        return OperationResult<UserDto>.SuccessResult(UserDto.From(user));
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
        if (user.CustomerId is { })
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
            Roles = user.Roles.Select(ur => ur.Role.ToString()).ToList(), // Correctly map Role enum
            IsEmailConfirmed = user.IsEmailConfirmed
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
            Roles = u.Roles.Select(r => r.Role.ToString()).ToList(), // Role should not be null
            IsEmailConfirmed = u.IsEmailConfirmed
        };

        return user is null
            ? OperationResult<UserDto>.FailureResult("User not found for the given Customer ID.")
            : OperationResult<UserDto>.SuccessResult(tempMapper(user)); // Use temp mapper
    }

}
