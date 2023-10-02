using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface IBusinessService
{
    Task<IReadOnlyList<Campaign>?> GetAllCampaignsAsync(Guid businessId);
    Task<Business> GetBusinessAsync(Guid businessId);
    Task<Campaign> GetCampaignAsync(Guid businessId, Guid campaignId);
    Task<Business> CreateBusinessAsync(Business newBusiness);
    Task<User> CreateBusinessUserAsync(User newBusinessUser);
    Task<Campaign> CreateCampaignAsync(Campaign newCampaign);
    Task<Business> UpdateBusinessAsync(Business updatedBusiness);
    Task UpdatePermissionsAsync(List<BusinessUserPermission> permissions);
    Task<Campaign> UpdateCampaignAsync(Campaign updatedCampaign);
    Task DeleteBusinessAsync(Guid businessId);
    Task DeleteCampaignAsync(Guid businessId, List<Guid> campaignIds);
}