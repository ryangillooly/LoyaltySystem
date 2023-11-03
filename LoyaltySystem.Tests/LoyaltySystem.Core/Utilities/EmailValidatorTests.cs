using System.Text.RegularExpressions;
using FluentAssertions;
using LoyaltySystem.Core.Utilities;
using Xunit;

namespace Tests.Core.Utilities.Tests
{
    public class EmailValidatorTests
    {
        [Theory]
        [InlineData("test@example.com", true)]
        [InlineData("user.name@example.com", true)]
        [InlineData("user.name@subdomain.example.com", true)]
        [InlineData("valid_email@domain.com", true)]
        [InlineData("test@example.co.uk", true)]
        [InlineData("test@example.com.", false)]        // trailing dot is invalid according to the regex
        [InlineData("test@.com.my", false)]             // leading dot in domain is invalid according to the regex
        [InlineData("test123@.com", false)]             // domain can't start with a dot according to the regex
        [InlineData("test123@.com.com", false)]         // domain can't start with a dot according to the regex
        [InlineData("test()*@example.com", false)]      // special characters not allowed by the regex
        [InlineData("test@%*.com", false)]              // special characters not allowed by the regex
        [InlineData("test@test@example.com", false)]    // multiple '@' not allowed by the regex
        [InlineData("", false)]                         // empty string is invalid according to the regex
        [InlineData("   ", false)]                      // whitespace is invalid according to the regex
        public void IsValidEmail_WhenCalled_ReturnsCorrectValidationResult(string email, bool expected)
        {
            // Assert
            email.IsValidEmail().Should().Be(expected);
        }
    }
}