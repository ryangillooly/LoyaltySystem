using LoyaltySystem.Domain.Common;
using System.Text.RegularExpressions;

namespace LoyaltySystem.Domain.ValueObjects;

/// <summary>
/// Represents personal information for customers with validation
/// Encapsulates name, contact details, and demographic information
/// </summary>
public record PersonalInfo
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    public string Phone { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public string? PreferredLanguage { get; init; }
    public Address? Address { get; init; }

    public PersonalInfo(
        string firstName,
        string lastName,
        string email,
        string phone,
        DateTime? dateOfBirth = null,
        string? preferredLanguage = null,
        Address? address = null)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required");
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required");
        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("Phone is required");

        // Validate name lengths
        if (firstName.Length < 2 || firstName.Length > 50)
            throw new DomainException("First name must be between 2 and 50 characters");
        if (lastName.Length < 2 || lastName.Length > 50)
            throw new DomainException("Last name must be between 2 and 50 characters");

        // Validate email format
        if (!IsValidEmail(email))
            throw new DomainException("Invalid email format");

        // Validate phone format (basic validation)
        if (!IsValidPhone(phone))
            throw new DomainException("Invalid phone format");

        // Validate date of birth
        if (dateOfBirth.HasValue)
        {
            if (dateOfBirth.Value > DateTime.Today)
                throw new DomainException("Date of birth cannot be in the future");
            if (dateOfBirth.Value < DateTime.Today.AddYears(-120))
                throw new DomainException("Date of birth cannot be more than 120 years ago");
        }

        // Validate preferred language (ISO 639-1 codes)
        if (!string.IsNullOrEmpty(preferredLanguage) && !IsValidLanguageCode(preferredLanguage))
            throw new DomainException("Invalid language code. Use ISO 639-1 format (e.g., 'en', 'es', 'fr')");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        Phone = phone.Trim();
        DateOfBirth = dateOfBirth;
        PreferredLanguage = preferredLanguage?.Trim().ToLowerInvariant();
        Address = address;
    }

    /// <summary>
    /// Gets the full name (first name + last name)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Calculates age based on date of birth
    /// Returns null if date of birth is not provided
    /// </summary>
    public int? Age
    {
        get
        {
            if (!DateOfBirth.HasValue) return null;
            
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Value.Year;
            
            if (DateOfBirth.Value.Date > today.AddYears(-age))
                age--;
                
            return age;
        }
    }

    /// <summary>
    /// Checks if the customer is a minor (under 18)
    /// Returns null if date of birth is not provided
    /// </summary>
    public bool? IsMinor => Age.HasValue ? Age < 18 : null;

    /// <summary>
    /// Validates email format using regex
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        const string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, emailPattern);
    }

    /// <summary>
    /// Validates phone format (basic validation for international formats)
    /// </summary>
    private static bool IsValidPhone(string phone)
    {
        // Remove common formatting characters
        var cleanPhone = Regex.Replace(phone, @"[\s\-\(\)\+]", "");
        
        // Check if it contains only digits and is reasonable length
        return Regex.IsMatch(cleanPhone, @"^\d{7,15}$");
    }

    /// <summary>
    /// Validates ISO 639-1 language codes
    /// </summary>
    private static bool IsValidLanguageCode(string languageCode)
    {
        if (string.IsNullOrEmpty(languageCode) || languageCode.Length != 2)
            return false;

        // Common ISO 639-1 language codes
        var validCodes = new HashSet<string>
        {
            "en", "es", "fr", "de", "it", "pt", "ru", "zh", "ja", "ko",
            "ar", "hi", "th", "vi", "nl", "sv", "da", "no", "fi", "pl"
        };

        return validCodes.Contains(languageCode.ToLowerInvariant());
    }

    /// <summary>
    /// Creates a new PersonalInfo with updated email
    /// </summary>
    public PersonalInfo WithEmail(string newEmail)
    {
        return new PersonalInfo(FirstName, LastName, newEmail, Phone, DateOfBirth, PreferredLanguage, Address);
    }

    /// <summary>
    /// Creates a new PersonalInfo with updated phone
    /// </summary>
    public PersonalInfo WithPhone(string newPhone)
    {
        return new PersonalInfo(FirstName, LastName, Email, newPhone, DateOfBirth, PreferredLanguage, Address);
    }

    /// <summary>
    /// Creates a new PersonalInfo with updated address
    /// </summary>
    public PersonalInfo WithAddress(Address? newAddress)
    {
        return new PersonalInfo(FirstName, LastName, Email, Phone, DateOfBirth, PreferredLanguage, newAddress);
    }
} 