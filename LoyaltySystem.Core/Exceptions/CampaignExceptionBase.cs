namespace LoyaltySystem.Core.Exceptions;

public abstract class CampaignExceptionBase : Exception
{
    public Guid CampaignId { get; }
    public Guid BusinessId { get; }

    protected CampaignExceptionBase(Guid campaignId, Guid businessId, string message)
        : base(message) =>
        (CampaignId, BusinessId) = (campaignId, businessId);
}