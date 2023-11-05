using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface IBusinessRepository
{
    // Business
    Task CreateBusinessAsync(Business entity, EmailToken emailToken);
    Task UpdateBusinessAsync(Business updatedBusinessAsync);
    Task<Business> GetBusinessAsync(Guid businessId);
    Task<List<Business>> GetBusinessesAsync(List<Guid> businessIdList);
    Task DeleteBusinessAsync(Guid businessId);
    
    
    // Business User Permissions
    Task CreateBusinessUserPermissionsAsync(List<BusinessUserPermissions> newBusinessUserPermissions);
    Task UpdateBusinessUserPermissionsAsync(List<BusinessUserPermissions> newBusinessUserPermissions);
    Task<List<BusinessUserPermissions>> GetBusinessPermissionsAsync(Guid businessId);
    Task<BusinessUserPermissions> GetBusinessUsersPermissionsAsync(Guid businessId, Guid userId);
    Task DeleteUsersPermissionsAsync(Guid businessId, List<Guid> userIdList);

    
    
    // Business Campaigns
    Task CreateCampaignAsync(Campaign campaign);
    Task<IReadOnlyList<Campaign>?> GetAllCampaignsAsync(Guid businessId);
    Task<Campaign> GetCampaignAsync(Guid businessId, Guid campaignId);
    Task UpdateCampaignAsync(Campaign updatedCampaign);
    Task DeleteCampaignAsync(Guid businessId, List<Guid> campaignIds);
}