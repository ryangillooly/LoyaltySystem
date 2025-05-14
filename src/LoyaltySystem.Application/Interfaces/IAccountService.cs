using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.PasswordReset;
using LoyaltySystem.Application.DTOs.Customers;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.Interfaces;

public interface IAccountService 
{
    Task<OperationResult<InternalUserDto>> RegisterAsync(RegisterUserDto registerDto, IEnumerable<RoleType> roles, bool createCustomer = false, CustomerExtraData? customerData = null);
    Task<OperationResult> VerifyEmailAsync(string token);
    Task<OperationResult> ResendVerificationEmailAsync(string email);
    Task<OperationResult> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task<OperationResult> ResetPasswordAsync(ResetPasswordRequestDto request);    
}
