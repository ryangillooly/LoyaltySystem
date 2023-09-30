using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface ILoyaltyCardService
{
    Task<IEnumerable<LoyaltyCard>> GetAllAsync();
    Task<LoyaltyCard> GetByIdAsync(Guid id, Guid userId);
    Task<LoyaltyCard> CreateLoyaltyCardAsync(Guid userId, Guid businessId);
}