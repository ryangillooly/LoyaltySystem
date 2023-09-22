using System.Text.RegularExpressions;

namespace LoyaltySystem.Core.Utilities;

public static class EmailValidator
{
    private static readonly Regex EmailPattern = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    public static bool IsValidEmail(this string email) => EmailPattern.IsMatch(email);
}
