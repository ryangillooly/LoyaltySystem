using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface IAuthService
{ 
    Task<ApiUser> Authenticate(string username, string password);
}