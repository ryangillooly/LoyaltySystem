namespace LoyaltySystem.Application.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
    Task SendConfirmationEmailAsync(string toEmail, string confirmationToken);
    Task SendEmailAsync(string toEmail, string subject, string body);
}