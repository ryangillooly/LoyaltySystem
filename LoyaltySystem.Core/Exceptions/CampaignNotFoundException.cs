namespace LoyaltySystem.Core.Exceptions;

public class CampaignNotFoundException : CampaignExceptionBase
{
    public CampaignNotFoundException(Guid campaignId, Guid businessId)
        : base(campaignId, businessId, $"The loyalty campaign {campaignId} for business {businessId} was not found.") { }
}