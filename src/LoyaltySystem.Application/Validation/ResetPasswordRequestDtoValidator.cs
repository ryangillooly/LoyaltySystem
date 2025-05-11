using FluentValidation;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.PasswordReset;

namespace LoyaltySystem.Application.Validation;

public class ResetPasswordRequestDtoValidator : AbstractValidator<ResetPasswordRequestDto>
{
    public ResetPasswordRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
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
    }
}
