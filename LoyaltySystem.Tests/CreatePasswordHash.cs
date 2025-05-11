using LoyaltySystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LoyaltySystem.Tests;

public class CreatePasswordHashTests 
{
    private readonly IPasswordHasher<User> _passwordHasher = new PasswordHasher<User>();
    
    [Fact]
    public async Task CreatePasswordHash()
    {
        var passwordHash = _passwordHasher.HashPassword(new User(), "admin");
        Console.WriteLine("PasswordHash: " + passwordHash);
    }
}
