namespace LoyaltySystem.Core.Interfaces;

public interface IEmailRepository
{
    Task<bool> DoesEmailExistAsync(string email);
}