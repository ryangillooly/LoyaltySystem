using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Application.Interfaces.Users;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using Serilog;
using System.Data;

namespace LoyaltySystem.Application.Services.Users;

public class UserService : IUserService 
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger _logger;

    public UserService(
        IUserRepository userRepository, 
        ICustomerRepository customerRepository,
        ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    }
    
    public async Task<OperationResult<UserDto>> GetByIdAsync(UserId userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            _logger.Warning("User not found for UserId: {UserId}", userId);
            return OperationResult<UserDto>.FailureResult("User not found.");
        }

        if (user.CustomerId is { })
        {
            var customer = await _customerRepository.GetByIdAsync(user.CustomerId);
            if (customer is null)
                _logger.Warning("User {UserId} has CustomerId {CustomerId}, but the customer record was not found.", user.PrefixedId, user.CustomerId.ToString());
        }
        
        return OperationResult<UserDto>.SuccessResult(UserDto.From(user));
    }
    public async Task<OperationResult<UserDto>> GetByEmailAsync(string userEmail)
    {
        var user = await _userRepository.GetByEmailAsync(userEmail);
        if (user is null)
        {
            _logger.Warning("User not found for Email: {Email}", userEmail);
            return OperationResult<UserDto>.FailureResult($"User not found with email {userEmail}");
        }

        if (user.CustomerId is { })
        {
            var customer = await _customerRepository.GetByIdAsync(user.CustomerId);
            if (customer is null)
                _logger.Warning("User {UserId} has CustomerId {CustomerId}, but the customer record was not found.", user.PrefixedId, user.CustomerId.ToString());
        }
                    
        return OperationResult<UserDto>.SuccessResult(UserDto.From(user));
    }
    public async Task<OperationResult<UserDto>> GetByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        
        return user is null
            ? OperationResult<UserDto>.FailureResult("User not found for the given Customer ID.")
            : OperationResult<UserDto>.SuccessResult(UserDto.From(user));
    }
    
    public async Task<OperationResult<UserDto>> GetUserByCustomerIdAsync(CustomerId customerId)
    {
        var user = await _userRepository.GetByCustomerIdAsync(customerId);
        
        return user is null
            ? OperationResult<UserDto>.FailureResult("User not found for the given Customer ID.")
            : OperationResult<UserDto>.SuccessResult(UserDto.From(user));
    }

    public async Task<OperationResult<UserDto>> AddAsync(User user, IDbTransaction? transaction = null)
    {
        ArgumentNullException.ThrowIfNull(user);
        await _userRepository.AddAsync(user);
        
        _logger.Information("User {UserId} added", user.Id);
        return OperationResult<UserDto>.SuccessResult(UserDto.From(user));
    }
    
    public async Task<OperationResult<UserDto>> UpdateAsync(UserId userId, UpdateUserDto updateDto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return OperationResult<UserDto>.FailureResult("User not found.");
        
        if (string.IsNullOrEmpty(updateDto.UserName) && string.IsNullOrEmpty(updateDto.Email))
            return OperationResult<UserDto>.FailureResult("No profile information provided for update.");
        
        if (!string.IsNullOrEmpty(updateDto.UserName))
            user.UpdateUserName(updateDto.UserName);
            
        if (!string.IsNullOrEmpty(updateDto.Email))
            user.UpdateEmail(updateDto.Email);
        
        if (updateDto.IsEmailConfirmed.HasValue)
            user.IsEmailConfirmed = updateDto.IsEmailConfirmed.Value;
        
        await _userRepository.UpdateAsync(user); 

        return OperationResult<UserDto>.SuccessResult(UserDto.From(user));
    }

    public async Task<OperationResult<UserDto>> DeleteAsync(UserId userId, UpdateUserDto updateDto) =>
        throw new NotImplementedException();
    
    public async Task<OperationResult> ConfirmEmailAndUpdateAsync(UserId userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return OperationResult.FailureResult("User not found.");

        if (user.IsEmailConfirmed)
            return OperationResult.SuccessResult(); 

        user.IsEmailConfirmed = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return OperationResult.SuccessResult();
    }
}
