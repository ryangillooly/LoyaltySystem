using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface IEmailRepository
{
    Task<bool> DoesEmailExistAsync(string email);
    Task SendEmailAsync(EmailInfo model);
}