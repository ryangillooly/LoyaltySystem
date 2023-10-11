using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Enums;
using static  LoyaltySystem.Core.Exceptions.LoyaltyCardExceptions;
using static  LoyaltySystem.Core.Exceptions.BusinessExceptions;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Interfaces;

namespace LoyaltySystem.Services;

public class LoyaltyCardService : ILoyaltyCardService
{
    private readonly ILoyaltyCardRepository _loyaltyCardRepository;
    private readonly IBusinessRepository _businessRepository;
    
    public LoyaltyCardService(ILoyaltyCardRepository loyaltyCardRepository, IBusinessRepository businessRepository) => 
        (_loyaltyCardRepository, _businessRepository) = (loyaltyCardRepository, businessRepository);

    public async Task<LoyaltyCard> CreateLoyaltyCardAsync(Guid userId, Guid businessId)
    {
        var newLoyaltyCard = new LoyaltyCard(userId, businessId);
        await _loyaltyCardRepository.CreateLoyaltyCardAsync(newLoyaltyCard);
        return newLoyaltyCard;
    }

    public async Task DeleteLoyaltyCardAsync(Guid userId, Guid businessId) => 
        await _loyaltyCardRepository.DeleteLoyaltyCardAsync(userId, businessId);
    public async Task<IEnumerable<LoyaltyCard>> GetLoyaltyCardsAsync(Guid userId) => 
        await _loyaltyCardRepository.GetLoyaltyCardsAsync(userId);
    public async Task<LoyaltyCard?> GetLoyaltyCardAsync(Guid userId, Guid businessId) =>
        await _loyaltyCardRepository.GetLoyaltyCardAsync(userId, businessId);
    
    public async Task<LoyaltyCard> UpdateLoyaltyCardAsync(Guid userId, Guid businessId, LoyaltyStatus status)
    {
        var currentRecord = await _loyaltyCardRepository.GetLoyaltyCardAsync(userId, businessId);

        var updatedLoyaltyCard = currentRecord;
        updatedLoyaltyCard!.Status = status;
        
        var mergedRecord = LoyaltyCard.Merge(currentRecord!, updatedLoyaltyCard);
            
        await _loyaltyCardRepository.UpdateLoyaltyCardAsync(mergedRecord);
            
        return mergedRecord;
    }

    public async Task<LoyaltyCard> StampLoyaltyCardAsync(Guid userId, Guid businessId)
    {
        var currentRecord = await _loyaltyCardRepository.GetLoyaltyCardAsync(userId, businessId);
        
        if (currentRecord!.Status != LoyaltyStatus.Active) throw new InactiveCardException(userId, businessId);

        currentRecord.Points += 1;
        currentRecord.LastStampedDate = DateTime.UtcNow;
        
        await _loyaltyCardRepository.StampLoyaltyCardAsync(currentRecord);
            
        return currentRecord;
    }

    public async Task<LoyaltyCard> RedeemLoyaltyCardRewardAsync(Guid userId, Guid businessId, Guid campaignId, Guid rewardId)
    {
        // Get Loyalty Card
        var loyaltyCard = await _loyaltyCardRepository.GetLoyaltyCardAsync(userId, businessId);
        
        // Validate that it's active
        if (loyaltyCard!.Status != LoyaltyStatus.Active) throw new InactiveCardException(userId, businessId);

        // Get Loyalty Campaign, and Rewards
        var campaign = await _businessRepository.GetCampaignAsync(businessId, campaignId);

        // Validate that it's active
        if (!campaign!.IsActive) throw new CampaignNotActiveException(businessId, campaignId);

        // Get specific reward from Campaign
        var selectedReward = campaign.Rewards.First(reward => reward.Id == rewardId);
        
        // If the card points are less than the reward point requirements, throw error
        if (loyaltyCard.Points < selectedReward.PointsRequired) throw new NotEnoughPointsException(userId, businessId, campaignId, rewardId, selectedReward);
        
        // Update Loyalty Card Redeem Date + Points
        loyaltyCard.LastRedeemDate = DateTime.UtcNow;
        loyaltyCard.Points        -= selectedReward.PointsRequired;
        
       // Redeem Reward 
       await _loyaltyCardRepository.RedeemLoyaltyCardRewardAsync(loyaltyCard, campaignId, rewardId);
       
        return loyaltyCard;
    }
}