using FluentValidation;
using LoyaltySystem.Application.DTOs.Auth;

namespace LoyaltySystem.Application.Validation;

public class ResendEmailVerificationRequestDtoValidator : AbstractValidator<RegisterUserRequestDto>
{
    /// <summary>
    /// Initializes validation rules for the <c>Email</c> property of <c>RegisterUserRequestDto</c>, requiring it to be non-empty and in a valid email format.
    /// </summary>
    public ResendEmailVerificationRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .When(x => !string.IsNullOrEmpty(x.Email));

    }
}
