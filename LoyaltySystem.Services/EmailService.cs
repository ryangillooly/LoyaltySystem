using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Utilities;

namespace LoyaltySystem.Services;

public class EmailService : IEmailService
{
    private readonly IEmailRepository _emailRepository;
    public EmailService(IEmailRepository emailRepository) => _emailRepository = emailRepository;

    public async Task<bool> IsEmailUnique(string email)
    {
        if (!IsValid(email)) throw new ArgumentException("Invalid email format.");
        return await _emailRepository.DoesEmailExistAsync(email);
    }

    public bool IsValid(string email) => email.IsValidEmail();
}
