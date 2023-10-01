using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface IBusinessRepository
{
    Task<IEnumerable<Business>> GetAllAsync();
    Task<Business?> GetBusinessAsync(Guid businessId);
    Task<Campaign?> GetCampaignAsync(Guid businessId, Guid campaignId);
    Task CreateBusinessAsync(Business entity);
    Task CreateCampaignAsync(Campaign campaign);
    Task UpdateBusinessAsync(Business updatedBusinessAsync);
    Task UpdatePermissionsAsync(List<Permission> permissions);
    Task DeleteBusinessAsync(Guid businessId);
    Task DeleteCampaignAsync(Guid businessId, Guid campaignId);
}