using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface IEmailService
{
    Task<bool> IsEmailUnique(string email);
    bool IsValid(string email);
    Task SendVerificationEmailAsync<T>(T item, EmailToken token);
    string GenerateSecureToken(int size = 32);
}