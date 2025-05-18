using FluentValidation;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.AuthDtos;

namespace LoyaltySystem.Application.Validation;

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
        
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("A valid email address is required.")
            .When(x => !string.IsNullOrEmpty(x.Email));
        
        RuleFor(x => x.Username)
            .Length(2, 100).WithMessage("Username must be between 2 and 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Username));
        
        RuleFor(x => x)
            // Ensures only either Email OR Username is present, but not both (^ == XOR Operator)
            .Must(x => !string.IsNullOrEmpty(x.Email) ^ !string.IsNullOrEmpty(x.Username))
            .WithMessage("Either Email or Username must be provided, but not both.")
            .Must(x => !string.IsNullOrEmpty(x.Email) || !string.IsNullOrEmpty(x.Username))
            .WithMessage("Either Email or Username is required.");
    }
} 