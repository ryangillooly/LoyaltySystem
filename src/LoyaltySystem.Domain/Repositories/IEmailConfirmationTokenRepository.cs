using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;

namespace LoyaltySystem.Domain.Repositories;

public interface IEmailConfirmationTokenRepository
{
    Task SaveAsync(EmailConfirmationToken token);
    Task<EmailConfirmationToken?> GetByTokenAsync(string token);
    Task UpdateAsync(EmailConfirmationToken token);
    Task InvalidateAllForUserAsync(Guid userId);
}