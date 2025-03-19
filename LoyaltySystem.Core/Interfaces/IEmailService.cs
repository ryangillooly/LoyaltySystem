using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface IEmailService
{
    Task<bool> IsEmailUnique(string email);
    bool IsValid(string email);
    Task SendEmail(EmailInfo model);
    string GenerateSecureToken(int size = 32);
}