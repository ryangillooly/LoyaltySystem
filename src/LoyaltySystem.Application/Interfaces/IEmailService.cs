namespace LoyaltySystem.Application.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
    Task SendEmailConfirmationEmailAsync(string toEmail, string confirmationToken);
    Task SendEmailAsync(string toEmail, string subject, string body);
}