using LoyaltySystem.Application.DTOs.Auth.PasswordReset;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Application.Interfaces.Auth;

public interface IPasswordResetService 
{
    Task<OperationResult> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task<OperationResult> ResetPasswordAsync(ResetPasswordRequestDto request);
}
