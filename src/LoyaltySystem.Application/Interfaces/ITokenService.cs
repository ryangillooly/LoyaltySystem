using LoyaltySystem.Domain.Entities;

namespace LoyaltySystem.Application.Interfaces;

public interface ITokenService
{
    Task<string> GeneratePasswordResetTokenAsync(User user);
    Task<bool> ValidatePasswordResetTokenAsync(User user, string token);
    Task InvalidatePasswordResetTokenAsync(User user, string token);
}