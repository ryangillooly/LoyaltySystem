using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.PasswordReset;
using LoyaltySystem.Application.DTOs.Customers;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.Interfaces;

public interface IAccountService 
{
    /// <summary>
/// Registers a new user with specified roles and optionally creates a customer profile with additional data.
/// </summary>
/// <param name="registerRequestDto">The registration details for the new user.</param>
/// <param name="roles">The roles to assign to the user.</param>
/// <param name="createCustomer">Whether to create a customer profile for the user.</param>
/// <param name="customerData">Optional extra data for the customer profile.</param>
/// <returns>An operation result containing the registration response data.</returns>
Task<OperationResult<RegisterUserResponseDto>> RegisterAsync(RegisterUserRequestDto registerRequestDto, IEnumerable<RoleType> roles, bool createCustomer = false, CustomerExtraData? customerData = null);
    /// <summary>
/// Verifies a user's email address using the provided verification token.
/// </summary>
/// <param name="token">The email verification token.</param>
/// <returns>An operation result indicating whether the email verification was successful.</returns>
Task<OperationResult> VerifyEmailAsync(string token);
    /// <summary>
/// Sends a new verification email to the specified email address.
/// </summary>
/// <param name="email">The email address to which the verification email will be sent.</param>
/// <returns>An operation result indicating whether the email was successfully resent.</returns>
Task<OperationResult> ResendVerificationEmailAsync(string email);
    /// <summary>
/// Initiates the password reset process for a user based on the provided request data.
/// </summary>
/// <param name="request">The details required to start the password reset workflow.</param>
/// <returns>An operation result indicating whether the password reset initiation was successful.</returns>
Task<OperationResult> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    /// <summary>
/// Resets a user's password using the provided reset password request data.
/// </summary>
/// <param name="request">The reset password request containing the necessary information to complete the password reset.</param>
/// <returns>An operation result indicating the outcome of the password reset process.</returns>
Task<OperationResult> ResetPasswordAsync(ResetPasswordRequestDto request);    
}
