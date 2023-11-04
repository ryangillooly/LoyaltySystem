using Newtonsoft.Json;
using LoyaltySystem.Core.Convertors;
using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Models;

[JsonConverter(typeof(EmailTokenConvertor))]
public class EmailToken
{
    public EmailToken() { }
    public EmailToken(Guid userId, string email) => (UserId, Email) = (userId, email);
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; } = Guid.Empty;
    public string Email { get; set; } = string.Empty;
    public EmailTokenStatus Status { get; set; } = EmailTokenStatus.Unverified;
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddHours(24);
}