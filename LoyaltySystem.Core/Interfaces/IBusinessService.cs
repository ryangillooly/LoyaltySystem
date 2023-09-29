using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface IBusinessService
{
    Task<IEnumerable<Business>> GetAllAsync();
    Task<Business> GetByIdAsync(Guid businessId);
    Task<Business> CreateAsync(Business newBusiness);
    Task<Business> UpdateBusinessAsync(Business updatedBusiness);
    Task DeleteAsync(Guid businessId);
    Task<Campaign> CreateCampaignAsync(Campaign newCampaign);
    Task UpdatePermissionsAsync(List<Permission> permissions);
}