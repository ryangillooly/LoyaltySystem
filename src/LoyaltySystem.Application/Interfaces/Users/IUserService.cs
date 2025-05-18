using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth.PasswordReset;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using System.Data;

namespace LoyaltySystem.Application.Interfaces.Users;

public interface IUserService 
{
    /// <summary>
/// Asynchronously retrieves a user by their unique identifier.
/// </summary>
/// <param name="userId">The unique identifier of the user.</param>
/// <returns>An operation result containing the user's internal data if found.</returns>
Task<OperationResult<InternalUserDto>> GetByIdAsync(UserId userId);
    /// <summary>
/// Retrieves a user by their email address asynchronously.
/// </summary>
/// <param name="email">The email address to search for.</param>
/// <returns>An operation result containing the user data if found.</returns>
Task<OperationResult<InternalUserDto>> GetByEmailAsync(string email);
    /// <summary>
/// Retrieves a user by their username.
/// </summary>
/// <param name="username">The username to search for.</param>
/// <returns>An operation result containing the user data if found.</returns>
Task<OperationResult<InternalUserDto>> GetByUsernameAsync(string username);
    /// <summary>
/// Retrieves a user by their phone number.
/// </summary>
/// <param name="phoneNumber">The phone number associated with the user.</param>
/// <returns>An operation result containing the user data if found.</returns>
Task<OperationResult<InternalUserDto>> GetByPhoneNumberAsync(string phoneNumber);
    /// <summary>
/// Retrieves a user associated with the specified customer ID.
/// </summary>
/// <param name="customerId">The unique identifier of the customer.</param>
/// <returns>An operation result containing the user data if found.</returns>
Task<OperationResult<InternalUserDto>> GetUserByCustomerIdAsync(CustomerId customerId);
    
    /// <summary>
/// Adds a new user to the system, optionally within a database transaction.
/// </summary>
/// <param name="user">The user entity to add.</param>
/// <param name="transaction">An optional database transaction for atomic operation.</param>
/// <returns>The result of the add operation, including the created user's data if successful.</returns>
Task<OperationResult<InternalUserDto>> AddAsync(User user, IDbTransaction? transaction = null);
    /// <summary>
/// Updates the details of a user identified by the specified user ID using the provided update request data.
/// </summary>
/// <param name="userId">The unique identifier of the user to update.</param>
/// <param name="updateRequestDto">The data containing updated user information.</param>
/// <returns>An operation result containing the updated user data if successful.</returns>
Task<OperationResult<InternalUserDto>> UpdateAsync(UserId userId, UpdateUserRequestDto updateRequestDto);
    /// <summary>
/// Deletes a user identified by the specified user ID, using details from the update request.
/// </summary>
/// <param name="userId">The unique identifier of the user to delete.</param>
/// <param name="updateRequestDto">Additional information relevant to the deletion process.</param>
/// <returns>The result of the delete operation, including user data if applicable.</returns>
Task<OperationResult<InternalUserDto>> DeleteAsync(UserId userId, UpdateUserRequestDto updateRequestDto);
    /// <summary>
/// Updates a user's password using the provided reset password information.
/// </summary>
/// <param name="internalUserDto">The user whose password is to be updated.</param>
/// <param name="resetDto">The reset password request containing new password details.</param>
/// <returns>The result of the password update operation, including the updated user data if successful.</returns>
Task<OperationResult<InternalUserDto>> UpdatePasswordAsync(InternalUserDto internalUserDto, ResetPasswordRequestDto resetDto);
    
    /// <summary>
/// Confirms a user's email address and updates their status accordingly.
/// </summary>
/// <param name="userId">The unique identifier of the user whose email is to be confirmed.</param>
/// <returns>An <see cref="OperationResult"/> indicating the outcome of the operation.</returns>
Task<OperationResult> ConfirmEmailAndUpdateAsync(UserId userId);
}
