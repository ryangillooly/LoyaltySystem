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

    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class with required repositories, password hasher, and logger.
    /// </summary>
    /// <param name="userRepository">Repository for user data access.</param>
    /// <param name="customerRepository">Repository for customer data access.</param>
    /// <param name="passwordHasher">Hasher for user password operations.</param>
    /// <param name="logger">Logger for recording service events.</param>
    /// <exception cref="ArgumentNullException">Thrown if any dependency is null.</exception>
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
    
    /// <summary>
    /// Retrieves a user by their unique identifier asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to retrieve.</param>
    /// <returns>An operation result containing the user DTO if found; otherwise, a failure result.</returns>
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
    /// <summary>
    /// Retrieves a user by email address asynchronously.
    /// </summary>
    /// <param name="userEmail">The email address of the user to retrieve.</param>
    /// <returns>An operation result containing the user data if found; otherwise, a failure result.</returns>
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
    /// <summary>
    /// Retrieves a user by username asynchronously.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <returns>An operation result containing the user DTO if found; otherwise, a failure result.</returns>
    public async Task<OperationResult<InternalUserDto>> GetByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        return user is null
            ? OperationResult<InternalUserDto>.FailureResult("User not found for the given Customer ID.")
            : OperationResult<InternalUserDto>.SuccessResult(InternalUserDto.From(user));
    }

    /// <summary>
    /// Retrieves a user by phone number asynchronously.
    /// </summary>
    /// <param name="phoneNumber">The phone number to search for.</param>
    /// <returns>An operation result containing the user DTO if found; otherwise, a failure result.</returns>
    public async Task<OperationResult<InternalUserDto>> GetByPhoneNumberAsync(string phoneNumber)
    {
        var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber);
        return user is null
            ? OperationResult<InternalUserDto>.FailureResult("User not found for the given Phone Number.")
            : OperationResult<InternalUserDto>.SuccessResult(InternalUserDto.From(user));
    }
    
    /// <summary>
    /// Retrieves a user associated with the specified customer ID.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <returns>An operation result containing the user DTO if found; otherwise, a failure result.</returns>
    public async Task<OperationResult<InternalUserDto>> GetUserByCustomerIdAsync(CustomerId customerId)
    {
        var user = await _userRepository.GetByCustomerIdAsync(customerId);
        return user is null
            ? OperationResult<InternalUserDto>.FailureResult("User not found for the given Customer ID.")
            : OperationResult<InternalUserDto>.SuccessResult(InternalUserDto.From(user));
    }

    /// <summary>
    /// Adds a new user to the system and returns the created user data.
    /// </summary>
    /// <param name="user">The user entity to add.</param>
    /// <param name="transaction">Optional database transaction context.</param>
    /// <returns>An operation result containing the added user's data.</returns>
    public async Task<OperationResult<InternalUserDto>> AddAsync(User user, IDbTransaction? transaction = null)
    {
        ArgumentNullException.ThrowIfNull(user);
        await _userRepository.AddAsync(user);
        
        _logger.Information("User {UserId} added", user.Id);
        return OperationResult<InternalUserDto>.SuccessResult(InternalUserDto.From(user));
    }
    
    /// <summary>
    /// Updates the specified user's details with the provided information.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to update.</param>
    /// <param name="updateRequestDto">The data transfer object containing updated user information.</param>
    /// <returns>An operation result containing the updated user DTO if successful, or a failure result if the user is not found.</returns>
    public async Task<OperationResult<InternalUserDto>> UpdateAsync(UserId userId, UpdateUserRequestDto updateRequestDto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return OperationResult<InternalUserDto>.FailureResult("User not found.");
        
        if (!string.IsNullOrEmpty(updateRequestDto.Username))
            user.UpdateUserName(updateRequestDto.Username);
            
        if (!string.IsNullOrEmpty(updateRequestDto.Email))
            user.UpdateEmail(updateRequestDto.Email);
        
        if (updateRequestDto.IsEmailConfirmed.HasValue)
            user.IsEmailConfirmed = updateRequestDto.IsEmailConfirmed.Value;
        
        await _userRepository.UpdateAsync(user); 

        return OperationResult<InternalUserDto>.SuccessResult(InternalUserDto.From(user));
    }

    /// <summary>
        /// Not implemented. Intended to delete a user by user ID and update request data.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to delete.</param>
        /// <param name="updateRequestDto">The update request data associated with the deletion.</param>
        /// <exception cref="NotImplementedException">Always thrown as this method is not implemented.</exception>
        public async Task<OperationResult<InternalUserDto>> DeleteAsync(UserId userId, UpdateUserRequestDto updateRequestDto) =>
        throw new NotImplementedException();

    /// <summary>
    /// Updates a user's password by hashing the new password and saving it to the repository.
    /// </summary>
    /// <param name="internalUserDto">The user whose password will be updated.</param>
    /// <param name="resetDto">Contains the new password to set.</param>
    /// <returns>An operation result containing the updated user DTO.</returns>
    public async Task<OperationResult<InternalUserDto>> UpdatePasswordAsync(InternalUserDto internalUserDto, ResetPasswordRequestDto resetDto)
    {
        internalUserDto.PasswordHash = _passwordHasher.HashPassword(internalUserDto, resetDto.NewPassword);
        await _userRepository.UpdatePasswordAsync(internalUserDto.Id, internalUserDto.PasswordHash);

        return OperationResult<InternalUserDto>.SuccessResult(internalUserDto);
    }
    
    /// <summary>
    /// Confirms a user's email address and updates their record if not already confirmed.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to confirm.</param>
    /// <returns>An <see cref="OperationResult"/> indicating success or failure.</returns>
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
