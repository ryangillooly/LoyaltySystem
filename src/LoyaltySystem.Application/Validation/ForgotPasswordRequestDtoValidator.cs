using FluentValidation;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.PasswordReset;

namespace LoyaltySystem.Application.Validation;

public class ForgotPasswordRequestDtoValidator : AbstractValidator<ForgotPasswordRequestDto>
{
    public ForgotPasswordRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .When(x => !string.IsNullOrEmpty(x.Email));
        
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required.")
            .Length(2, 100).WithMessage("Username must be between 2 and 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.UserName));
        
        RuleFor(x => x)
            // Ensures only either Email OR Username is present, but not both (^ == XOR Operator)
            .Must(x => !string.IsNullOrEmpty(x.Email) ^ !string.IsNullOrEmpty(x.UserName))
            .WithMessage("Either Email or Username must be provided, but not both.")
            .Must(x => !string.IsNullOrEmpty(x.Email) || !string.IsNullOrEmpty(x.UserName))
            .WithMessage("Either Email or Username is required.");
    }
}
