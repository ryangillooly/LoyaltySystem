using Newtonsoft.Json;
using LoyaltySystem.Core.Convertors;
using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Models;

public class EmailToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public EmailTokenStatus Status { get; set; } = EmailTokenStatus.Unverified;
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddHours(24);
}

[JsonConverter(typeof(UserEmailTokenConvertor))]
public class UserEmailToken : EmailToken
{
    public UserEmailToken() { }
    public UserEmailToken(Guid userId, string email) => (UserId, Email) = (userId, email);
    public Guid UserId { get; set; }
}

[JsonConverter(typeof(BusinessEmailTokenConvertor))]
public class BusinessEmailToken : EmailToken
{
    public BusinessEmailToken() { }
    public BusinessEmailToken(Guid businessId, string email) => (BusinessId, Email) = (businessId, email);
    public Guid BusinessId { get; set; }
}