using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.DTOs;

public class VerifyEmailDto
{
    public VerifyEmailDto(Guid userId, Guid token) =>
        (UserId, Token) = (userId, token);
    
    public Guid UserId { get; set; }
    public Guid Token { get; set; }
}