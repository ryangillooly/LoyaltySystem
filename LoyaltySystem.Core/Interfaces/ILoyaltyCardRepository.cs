using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface ILoyaltyCardRepository
{
    Task<LoyaltyCard> GetByIdAsync(Guid id);
    Task<IEnumerable<LoyaltyCard>> GetAllAsync();
    Task AddAsync(LoyaltyCard loyaltyCard);
    Task UpdateAsync(LoyaltyCard loyaltyCard);
    Task DeleteAsync(Guid id);
    Task StampAsync(Guid id);
    Task RedeemAsync(Guid id);
}