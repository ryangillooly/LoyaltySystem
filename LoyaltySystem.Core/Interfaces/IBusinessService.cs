using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface IBusinessService
{
    // Business
    Task<Business> CreateBusinessAsync(Business newBusiness);
    Task<Business> UpdateBusinessAsync(Business updatedBusiness);
    Task DeleteBusinessAsync(Guid businessId);
    Task<List<Business>> GetBusinessesAsync(List<Guid> businessIdList);
    Task<Business> GetBusinessAsync(Guid businessId);
    
    
    // Business Users
    Task<List<BusinessUserPermissions>> CreateBusinessUserPermissionsAsync(List<BusinessUserPermissions> newBusinessUserPermissions);
    Task<List<BusinessUserPermissions>> GetBusinessPermissionsAsync(Guid businessId);
    Task<BusinessUserPermissions> GetBusinessUsersPermissionsAsync(Guid businessId, Guid userId);
    Task<List<BusinessUserPermissions>> UpdateBusinessUsersPermissionsAsync(List<BusinessUserPermissions> updatedBusinessUserPermissions);
    Task DeleteBusinessUsersPermissionsAsync(Guid businessId, List<Guid> userIdList);
    
    // Business Campaigns
    Task<Campaign> CreateCampaignAsync(Campaign newCampaign);
    Task<Campaign> UpdateCampaignAsync(Campaign updatedCampaign);
    Task DeleteCampaignAsync(Guid businessId, List<Guid> campaignIds);
    Task<Campaign> GetCampaignAsync(Guid businessId, Guid campaignId);
    Task<IReadOnlyList<Campaign>?> GetAllCampaignsAsync(Guid businessId);
    
    
}