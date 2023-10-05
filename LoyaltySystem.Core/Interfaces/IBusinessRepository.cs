using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface IBusinessRepository
{
    // Business
    Task CreateBusinessAsync(Business entity);
    Task UpdateBusinessAsync(Business updatedBusinessAsync);
    Task<Business?> GetBusinessAsync(Guid businessId);
    Task DeleteBusinessAsync(Guid businessId);
    
    
    // Business User Permissions
    Task CreateBusinessUserPermissionsAsync(List<BusinessUserPermissions> newBusinessUserPermissions);
    Task UpdateBusinessUserPermissionsAsync(List<BusinessUserPermissions> newBusinessUserPermissions);
    
    
    // Business Campaigns
    Task CreateCampaignAsync(Campaign campaign);
    Task<IReadOnlyList<Campaign>?> GetAllCampaignsAsync(Guid businessId);
    Task<Campaign?> GetCampaignAsync(Guid businessId, Guid campaignId);
    Task UpdateCampaignAsync(Campaign updatedCampaign);
    Task DeleteCampaignAsync(Guid businessId, List<Guid> campaignIds);
}