namespace LoyaltySystem.Application.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
    Task SendEmailAsync(string toEmail, string subject, string body);
}