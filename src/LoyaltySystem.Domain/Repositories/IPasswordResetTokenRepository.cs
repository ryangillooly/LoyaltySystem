using LoyaltySystem.Domain.Common;
using LoyaltySystem.Infrastructure.Entities;

namespace LoyaltySystem.Domain.Repositories;

public interface IPasswordResetTokenRepository
{
    Task SaveAsync(PasswordResetToken token);
    Task<PasswordResetToken?> GetByUserIdAndTokenAsync(UserId userId, string token);
    Task UpdateAsync(PasswordResetToken token);
    Task InvalidateAllForUserAsync(Guid userId); 
}
