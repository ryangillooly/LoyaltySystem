using System.Security.Cryptography;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Settings;
using LoyaltySystem.Core.Utilities;
using static LoyaltySystem.Core.Models.Constants;

namespace LoyaltySystem.Services;

public class EmailService : IEmailService
{
    private readonly IEmailRepository _emailRepository;
    private readonly EmailSettings _emailSettings;
    
    public EmailService(IEmailRepository emailRepository, EmailSettings emailSettings) => 
        (_emailRepository, _emailSettings) = (emailRepository, emailSettings);

    public async Task<bool> IsEmailUnique(string email)
    {
        if (!IsValid(email)) throw new ArgumentException("Invalid email format.");
        return await _emailRepository.DoesEmailExistAsync(email);
    }

    public bool IsValid(string email) => email.IsValidEmail();
    public string GenerateSecureToken(int length = 32)
    {
        // Because a Base64 character represents 6 bits, and a byte is 8 bits,
        // we need 3 bytes to represent 4 Base64 characters.
        var byteLength = (int)Math.Ceiling(length * 0.75);
        
        var tokenData = new byte[byteLength];
        RandomNumberGenerator.Fill(tokenData);
        
        // We take a substring in case the Base64 encoding is longer than the specified length.
        return Convert.ToBase64String(tokenData).Substring(0, length);
    }
    public async Task SendVerificationEmailAsync<T>(T item, EmailToken token)
    {
        string itemId;
        string itemType;
        
        switch(item)
        {
            case User user:
                itemId = user.Id.ToString();
                itemType = nameof(user);
                break;
            case Business business:
                itemId = business.Id.ToString();
                itemType = nameof(business);
                break;
            default:
                throw new Exception($"Unknown type provided to method {nameof(SendVerificationEmailAsync)}");
        }
        
        var verificationLink = $"{WebAddress}/{itemType}/{itemId}/verify-email/{token.Id}";
        var emailInfo = new EmailInfo
        {
            ToEmail   = token.Email,
            FromEmail = _emailSettings.From,
            Subject   = "Loyalty System - Verification",
            Body      = $"Please verify your {itemType} account by going to the following URL - {verificationLink}"
        };
        try
        {
            await _emailRepository.SendEmailAsync(emailInfo);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to send verification email - {ex}");
        }
    }

    
}
