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
    Task<Business> GetBusinessAsync(Guid businessId);
    
    
    // Business Users
    Task<List<BusinessUserPermissions>> CreateBusinessUserPermissionsAsync(List<BusinessUserPermissions> newBusinessUserPermissions);
    Task<List<BusinessUserPermissions>> UpdateBusinessUserPermissionsAsync(List<BusinessUserPermissions> updatedBusinessUserPermissions);
    Task<List<BusinessUserPermissions>> GetBusinessPermissionsAsync(Guid businessId);
    Task<BusinessUserPermissions> GetBusinessUsersPermissionsAsync(Guid businessId, Guid userId);
    
    
    // Business Campaigns
    Task<Campaign> CreateCampaignAsync(Campaign newCampaign);
    Task<Campaign> UpdateCampaignAsync(Campaign updatedCampaign);
    Task DeleteCampaignAsync(Guid businessId, List<Guid> campaignIds);
    Task<Campaign> GetCampaignAsync(Guid businessId, Guid campaignId);
    Task<IReadOnlyList<Campaign>?> GetAllCampaignsAsync(Guid businessId);
    
    
}