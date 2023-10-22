using System.Security.Cryptography;
using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
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
    public async Task SendEmail(EmailInfo model)
    {
        var client = new AmazonSimpleEmailServiceClient(RegionEndpoint.EUWest1); // Change the region if necessary

        var sendRequest = new SendEmailRequest
        {
            Source      = model.FromEmail,
            Destination = new Destination { ToAddresses = { model.ToEmail }},
            Message     = new Message
            {
                Subject = new Content(model.Subject),
                Body    = new Body { Text = new Content { Charset = "UTF-8", Data = model.Body }}
            }
        };

        try
        {
            Console.WriteLine("Sending email using Amazon SES...");
            var response = await client.SendEmailAsync(sendRequest);
            Console.WriteLine("Email sent successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending email: " + ex.Message);
        }
    }

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
}
