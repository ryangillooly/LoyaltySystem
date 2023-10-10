using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface ILoyaltyCardRepository
{
    Task<LoyaltyCard?> GetLoyaltyCardAsync(Guid userId, Guid businessId);
    Task<IEnumerable<LoyaltyCard>> GetAllAsync();
    Task CreateLoyaltyCardAsync(LoyaltyCard loyaltyCard);
    Task<Redemption> RedeemRewardAsync(Redemption redemption);
    Task UpdateLoyaltyCardAsync(LoyaltyCard updatedLoyaltyCard);
    Task DeleteLoyaltyCardAsync(Guid userId, Guid businessId);
    Task StampLoyaltyCardAsync(LoyaltyCard loyaltyCard);
    Task RedeemLoyaltyCardRewardAsync(LoyaltyCard loyaltyCard, Guid campaignId, Guid rewardId);
}