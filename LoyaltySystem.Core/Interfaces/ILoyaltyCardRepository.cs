using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface ILoyaltyCardRepository
{
    Task<LoyaltyCard> GetByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<LoyaltyCard>> GetAllAsync();
    Task<LoyaltyCard> CreateAsync(LoyaltyCard loyaltyCard);
    Task<Redemption> RedeemRewardAsync(Redemption redemption);
    Task UpdateAsync(LoyaltyCard loyaltyCard);
    Task DeleteAsync(Guid id);
}