using FluentValidation;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.PasswordReset;

namespace LoyaltySystem.Application.Validation;

public class ResetPasswordRequestDtoValidator : AbstractValidator<ResetPasswordRequestDto>
{
    public ResetPasswordRequestDtoValidator()
    {
        RuleFor(x => x.UserName)
            .Length(2, 100).WithMessage("Username must be between 2 and 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.UserName));
        
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("A valid email address is required.")
            .When(x => !string.IsNullOrEmpty(x.Email));
        
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");
        
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("NewPassword is required.")
            .Length(6, 100).WithMessage("NewPassword must be between 6 and 100 characters.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("ConfirmPassword is required.")
            .Equal(x => x.NewPassword).WithMessage("ConfirmPassword does not match.");
        
        RuleFor(x => x)
            // Ensures only either Email OR Username is present, but not both (^ == XOR Operator)
            .Must(x => !string.IsNullOrEmpty(x.Email) ^ !string.IsNullOrEmpty(x.UserName))
            .WithMessage("Either Email or Username must be provided, but not both.")
            .Must(x => !string.IsNullOrEmpty(x.Email) || !string.IsNullOrEmpty(x.UserName))
            .WithMessage("Either Email or Username is required.");
    }
}
