using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Exceptions;

public class LoyaltyCardExceptions
{
    public abstract class LoyaltyCardExceptionBase : Exception
    {
        private Guid UserId { get; }
        private Guid BusinessId { get; }
        private Guid CampaignId { get; }
        private Guid RewardId { get; }
        private Reward Reward { get; }

        protected LoyaltyCardExceptionBase(Guid userId, Guid businessId, string message)
            : base(message) => (UserId, BusinessId) = (userId, businessId);

        protected LoyaltyCardExceptionBase(Guid userId, Guid businessId, Guid campaignId, Guid rewardId,
            Reward reward, string message)
            : base(message) =>
            (UserId, BusinessId, CampaignId, RewardId, Reward) =
            (userId, businessId, campaignId, rewardId, reward);
    }

    public class InactiveCardException : LoyaltyCardExceptionBase
    {
        public InactiveCardException(Guid userId, Guid businessId)
            : base(userId, businessId, $"The loyalty card for user {userId}, for business {businessId}, is inactive and cannot be stamped.") { }
    }
    
    public class CardNotFoundException : LoyaltyCardExceptionBase
    {
        public CardNotFoundException(Guid userId, Guid businessId)
            : base(userId, businessId, $"The loyalty card for user {userId}, for business {businessId}, was not found.") { }
    }

    public class NotEnoughPointsException : LoyaltyCardExceptionBase
    {
        public NotEnoughPointsException(Guid userId, Guid businessId, Guid campaignId, Guid rewardId, Reward reward)
            : base(userId, businessId, $"The loyalty card for user {userId}, for business {businessId}, did not have enough points to redeem {reward.Title} ({reward.Id}). Campain {campaignId}") { }
    }
}