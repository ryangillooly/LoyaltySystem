using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.DTOs;

public class VerifyEmailDto
{
    public Guid Token { get; set; }
}

public class VerifyBusinessEmailDto : VerifyEmailDto
{
    public VerifyBusinessEmailDto(Guid businessId, Guid token) => (Token, BusinessId) = (token, businessId);
    public Guid BusinessId { get; set; }
}

public class VerifyUserEmailDto : VerifyEmailDto
{   
    public VerifyUserEmailDto(Guid userId, Guid token) => (Token, UserId) = (token, userId);
    public Guid UserId { get; set; }
}