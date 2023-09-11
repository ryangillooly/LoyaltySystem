using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface ILoyaltyCardRepository
{
    Task<LoyaltyCard> GetByIdAsync(Guid id, string userEmail);
    Task<IEnumerable<LoyaltyCard>> GetAllAsync();
    Task<LoyaltyCard> AddAsync(LoyaltyCard loyaltyCard);
    Task UpdateAsync(LoyaltyCard loyaltyCard);
    Task DeleteAsync(Guid id);
}