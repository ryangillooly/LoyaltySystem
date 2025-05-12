using LoyaltySystem.Domain.Entities;

namespace LoyaltySystem.Application.Interfaces.Auth;

public interface IPasswordResetTokenService
{
    Task<string> GenerateTokenAsync(User user);
    Task<bool> ValidateTokenAsync(User user, string token);
    Task InvalidateTokenAsync(User user, string token);
}