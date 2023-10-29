using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Services;

public class AuthService : IAuthService
{
    public async Task<ApiUser> Authenticate(string username, string password)
    {
        return new ApiUser();
    }
}