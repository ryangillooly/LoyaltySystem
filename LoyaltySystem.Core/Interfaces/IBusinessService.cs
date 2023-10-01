using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface IBusinessService
{
    Task<IEnumerable<Business>> GetAllAsync();
    Task<Business> GetBusinessAsync(Guid businessId);
    Task<Campaign> GetCampaignAsync(Guid businessId, Guid campaignId);
    Task<Business> CreateBusinessAsync(Business newBusiness);
    Task<Campaign> CreateCampaignAsync(Campaign newCampaign);
    Task<Business> UpdateBusinessAsync(Business updatedBusiness);
    Task UpdatePermissionsAsync(List<Permission> permissions);
    Task DeleteBusinessAsync(Guid businessId);
    Task DeleteCampaignAsync(Guid businessId, Guid campaignId);
}