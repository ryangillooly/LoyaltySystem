using FluentValidation;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.AuthDtos;

namespace LoyaltySystem.Application.Validation;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserRequestDto>
{
    /// <summary>
    /// Initializes validation rules for updating a user, ensuring that either a valid email or a valid username is provided.
    /// </summary>
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("A valid email address is required.")
            .When(x => !string.IsNullOrEmpty(x.Email));
        
        RuleFor(x => x.Username)
            .Length(2, 100).WithMessage("Username must be between 2 and 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Username));
        
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.Email) || !string.IsNullOrEmpty(x.Username))
            .WithMessage("Either Email or Username is required.");
    }
}
