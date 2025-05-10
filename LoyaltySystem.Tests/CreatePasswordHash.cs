namespace LoyaltySystem.Tests;

public static class CreatePasswordHashTests 
{
    [Theory, InlineData("admin")]
    private static void CreatePasswordHash(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        using var hmac = new System.Security.Cryptography.HMACSHA512();
        var passwordSalt = Convert.ToBase64String(hmac.Key);
        var passwordHash = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));

        var pause = new List<string>();
    }
}