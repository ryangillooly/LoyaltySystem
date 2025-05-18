namespace LoyaltySystem.Application.Interfaces;

public interface IEmailService
{
    /// <summary>
/// Sends a password reset email to the specified address using the provided reset token.
/// </summary>
/// <param name="toEmail">The recipient's email address.</param>
/// <param name="resetToken">The token to include for password reset verification.</param>
Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
    /// <summary>
/// Sends an account confirmation email to the specified address using the provided confirmation token.
/// </summary>
/// <param name="toEmail">The recipient's email address.</param>
/// <param name="confirmationToken">The token used to confirm the account.</param>
Task SendConfirmationEmailAsync(string toEmail, string confirmationToken);
    /// <summary>
/// Sends an email with the specified subject and body to the given recipient asynchronously.
/// </summary>
/// <param name="toEmail">The recipient's email address.</param>
/// <param name="subject">The subject of the email.</param>
/// <param name="body">The content of the email message.</param>
Task SendEmailAsync(string toEmail, string subject, string body);
}