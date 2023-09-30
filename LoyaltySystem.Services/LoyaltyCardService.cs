using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Interfaces;

namespace LoyaltySystem.Services;

public class LoyaltyCardService : ILoyaltyCardService
{
    private readonly ILoyaltyCardRepository _loyaltyCardRepository;
    
    public LoyaltyCardService(ILoyaltyCardRepository loyaltyCardRepository) => 
        (_loyaltyCardRepository) = (loyaltyCardRepository);

    public async Task<LoyaltyCard> CreateLoyaltyCardAsync(Guid userId, Guid businessId)
    {
        var newLoyaltyCard = new LoyaltyCard(userId, businessId);
        await _loyaltyCardRepository.CreateLoyaltyCardAsync(newLoyaltyCard);
        return newLoyaltyCard;
    }

    public async Task<IEnumerable<LoyaltyCard>> GetAllAsync() => await _loyaltyCardRepository.GetAllAsync();
    public async Task<LoyaltyCard> GetByIdAsync(Guid id, Guid userId) => await _loyaltyCardRepository.GetByIdAsync(id, userId);
}