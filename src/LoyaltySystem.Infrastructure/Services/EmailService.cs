using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Shared.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace LoyaltySystem.Infrastructure.Services;

public class EmailService : IEmailService 
{
    private readonly SmtpSettings _smtpSettings;
    private readonly SmtpClient _smtpClient;

    public EmailService(IOptions<SmtpSettings> smtpSettings)
    {
        _smtpSettings = smtpSettings.Value;
        _smtpClient = new SmtpClient
        {
            Host = _smtpSettings.Host,
            Port = _smtpSettings.Port,
            Credentials = new NetworkCredential(_smtpSettings.User, _smtpSettings.Password),
            EnableSsl = _smtpSettings.EnableSsl,
        };
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken) =>
        await SendEmailAsync(
            toEmail, 
            "Password Reset Request", 
            $"To reset your password, use this token: {resetToken}"
        );
    
    public async Task SendConfirmationEmailAsync(string toEmail, string confirmationToken) =>
        await SendEmailAsync(
            toEmail, 
            "Email Verification", 
            $"To verify your email, use this token: {confirmationToken}"
        );

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var mailMessage = new MailMessage(_smtpSettings.FromAddress, toEmail, subject, body);
        await _smtpClient.SendMailAsync(mailMessage);
    }
}
