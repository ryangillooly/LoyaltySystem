using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth.PasswordReset;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Application.Interfaces.Users;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Data;

namespace LoyaltySystem.Application.Services.Users;

public class UserService : IUserService 
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IPasswordHasher<InternalUserDto> _passwordHasher;
    private readonly ILogger _logger;

    public UserService(
        IUserRepository userRepository, 
        ICustomerRepository customerRepository,
        IPasswordHasher<InternalUserDto> passwordHasher,
        ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    }
    
    public async Task<OperationResult<InternalUserDto>> GetByIdAsync(UserId userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            _logger.Warning("User not found for UserId: {UserId}", userId);
            return OperationResult<InternalUserDto>.FailureResult("User not found.");
        }

        if (user.CustomerId is { })
        {
            var customer = await _customerRepository.GetByIdAsync(user.CustomerId);
            if (customer is null)
                _logger.Warning("User {UserId} has CustomerId {CustomerId}, but the customer record was not found.", user.PrefixedId, user.CustomerId.ToString());
        }
        
        return OperationResult<InternalUserDto>.SuccessResult(InternalUserDto.From(user));
    }
    public async Task<OperationResult<InternalUserDto>> GetByEmailAsync(string userEmail)
    {
        var user = await _userRepository.GetByEmailAsync(userEmail);
        if (user is null)
        {
            _logger.Warning("User not found for Email: {Email}", userEmail);
            return OperationResult<InternalUserDto>.FailureResult($"User not found with email {userEmail}");
        }

        if (user.CustomerId is { })
        {
            var customer = await _customerRepository.GetByIdAsync(user.CustomerId);
            if (customer is null)
                _logger.Warning("User {UserId} has CustomerId {CustomerId}, but the customer record was not found.", user.PrefixedId, user.CustomerId.ToString());
        }
                    
        return OperationResult<InternalUserDto>.SuccessResult(InternalUserDto.From(user));
    }
    public async Task<OperationResult<InternalUserDto>> GetByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        return user is null
            ? OperationResult<InternalUserDto>.FailureResult("User not found for the given Customer ID.")
            : OperationResult<InternalUserDto>.SuccessResult(InternalUserDto.From(user));
    }

    public async Task<OperationResult<InternalUserDto>> GetByPhoneNumberAsync(string phoneNumber)
    {
        var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber);
        return user is null
            ? OperationResult<InternalUserDto>.FailureResult("User not found for the given Phone Number.")
            : OperationResult<InternalUserDto>.SuccessResult(InternalUserDto.From(user));
    }
    
    public async Task<OperationResult<InternalUserDto>> GetUserByCustomerIdAsync(CustomerId customerId)
    {
        var user = await _userRepository.GetByCustomerIdAsync(customerId);
        return user is null
            ? OperationResult<InternalUserDto>.FailureResult("User not found for the given Customer ID.")
            : OperationResult<InternalUserDto>.SuccessResult(InternalUserDto.From(user));
    }

    public async Task<OperationResult<InternalUserDto>> AddAsync(User user, IDbTransaction? transaction = null)
    {
        ArgumentNullException.ThrowIfNull(user);
        await _userRepository.AddAsync(user);
        
        _logger.Information("User {UserId} added", user.Id);
        return OperationResult<InternalUserDto>.SuccessResult(InternalUserDto.From(user));
    }
    
    public async Task<OperationResult<InternalUserDto>> UpdateAsync(UserId userId, UpdateUserRequestDto updateRequestDto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return OperationResult<InternalUserDto>.FailureResult("User not found.");
        
        if (!string.IsNullOrEmpty(updateRequestDto.UserName))
            user.UpdateUserName(updateRequestDto.UserName);
            
        if (!string.IsNullOrEmpty(updateRequestDto.Email))
            user.UpdateEmail(updateRequestDto.Email);
        
        if (updateRequestDto.IsEmailConfirmed.HasValue)
            user.IsEmailConfirmed = updateRequestDto.IsEmailConfirmed.Value;
        
        await _userRepository.UpdateAsync(user); 

        return OperationResult<InternalUserDto>.SuccessResult(InternalUserDto.From(user));
    }

    public async Task<OperationResult<InternalUserDto>> DeleteAsync(UserId userId, UpdateUserRequestDto updateRequestDto) =>
        throw new NotImplementedException();

    public async Task<OperationResult<InternalUserDto>> UpdatePasswordAsync(InternalUserDto internalUserDto, ResetPasswordRequestDto resetDto)
    {
        internalUserDto.PasswordHash = _passwordHasher.HashPassword(internalUserDto, resetDto.NewPassword);
        await _userRepository.UpdatePasswordAsync(internalUserDto.Id, internalUserDto.PasswordHash);

        return OperationResult<InternalUserDto>.SuccessResult(internalUserDto);
    }
    
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
