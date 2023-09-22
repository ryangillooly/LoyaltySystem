namespace LoyaltySystem.Core.Interfaces;

public interface IEmailService
{
    Task<bool> IsEmailUnique(string email);
    bool IsValid(string email);
}