using System.Text.RegularExpressions;

namespace LoyaltySystem.Core.Utilities;

public static class EmailValidator
{
    private static readonly Regex EmailPattern = new Regex(
        @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public static bool IsValidEmail(this string email) => EmailPattern.IsMatch(email);
}
