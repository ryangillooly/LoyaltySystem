using FluentValidation;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.AuthDtos;

namespace LoyaltySystem.Application.Validation;

public class UpdateProfileDtoValidator : AbstractValidator<UpdateUserRequestDto>
{
    private const string PhoneRegex = @"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,9}$";
   
    /// <summary>
    /// Initializes validation rules for updating user profile data, including email, username, phone number, and password fields.
    /// </summary>
    /// <remarks>
    /// Applies conditional validation for email, username, and phone fields only if they are provided. Enforces required and format constraints for new password and confirmation fields.
    /// </remarks>
    public UpdateProfileDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .When(x => !string.IsNullOrEmpty(x.Email));
        
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Length(2, 100).WithMessage("Username must be between 2 and 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Username));
        
        RuleFor(x => x.Phone)
            .MinimumLength(7).WithMessage("Phone number must be at least 7 digits.") // Adjust min length if needed
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.") // Adjust max length
            .Matches(PhoneRegex).WithMessage("Invalid phone number format.")
            .When(x => !string.IsNullOrEmpty(x.Phone)); // Only validate if not empty
        
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("NewPassword is required.")
            .Length(5, 100).WithMessage("NewPassword must be between 5 and 100 characters.");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("ConfirmNewPassword is required.")
            .Equal(x => x.NewPassword).WithMessage("ConfirmNewPassword does not match.");
    }
}
