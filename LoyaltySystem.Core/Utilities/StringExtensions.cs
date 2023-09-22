using System.Globalization;

namespace LoyaltySystem.Core.Utilities;

public static class StringExtensions
{
    public static string ToPascalCase(this string text)
    {
        // Use TextInfo.ToTitleCase to get proper casing
        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        text = textInfo.ToTitleCase(text.ToLower());

        // Remove any non-letter character and combine the words to get PascalCase
        return new string(text.Where(char.IsLetterOrDigit).ToArray());
    }
}