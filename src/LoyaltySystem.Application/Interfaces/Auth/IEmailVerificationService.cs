using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;

namespace LoyaltySystem.Application.Interfaces.Auth;

public interface IEmailVerificationService 
{
    Task<OperationResult> VerifyEmailAsync(string token);
    Task<string> GenerateTokenAsync(User user);
    Task<bool> ValidateTokenAsync(User user, string token);
    Task InvalidateTokenAsync(User user, string token);
}
