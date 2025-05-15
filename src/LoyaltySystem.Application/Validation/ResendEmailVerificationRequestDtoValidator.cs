using FluentValidation;
using LoyaltySystem.Application.DTOs.Auth;

namespace LoyaltySystem.Application.Validation;

public class ResendEmailVerificationRequestDtoValidator : AbstractValidator<RegisterUserRequestDto>
{
    public ResendEmailVerificationRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .When(x => !string.IsNullOrEmpty(x.Email));

    }
}
