using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface ILoyaltyCardService
{
    Task<IEnumerable<LoyaltyCard>> GetAllAsync();
    Task<LoyaltyCard?> GetLoyaltyCardAsync(Guid userId, Guid businessId);
    Task<LoyaltyCard> CreateLoyaltyCardAsync(Guid userId, Guid businessId);
    Task DeleteLoyaltyCardAsync(Guid userId, Guid businessId);
    Task<LoyaltyCard> UpdateLoyaltyCardAsync(Guid userId, Guid businessId, LoyaltyStatus status);
    Task<LoyaltyCard> StampLoyaltyCardAsync(Guid userId, Guid businessId);
    Task<LoyaltyCard> RedeemLoyaltyCardRewardAsync(Guid userId, Guid businessId, Guid campaignId, Guid rewardId);
}