using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface IBusinessRepository
{
    Task<IEnumerable<Business>> GetAllAsync();
    Task<Business?> GetBusinessAsync(Guid businessId);
    Task CreateBusinessAsync(Business entity);
    Task CreateCampaignAsync(Campaign campaign);
    Task UpdateBusinessAsync(Business updatedBusinessAsync);
    Task DeleteBusinessAsync(Guid businessId);
    Task UpdatePermissionsAsync(List<Permission> permissions);
}