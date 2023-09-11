using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Interfaces;

namespace LoyaltySystem.Services;

public class LoyaltyCardService : ILoyaltyCardService
{
    private readonly ILoyaltyCardRepository _loyaltyCardRepository;
    public LoyaltyCardService(ILoyaltyCardRepository loyaltyCardRepository) => _loyaltyCardRepository = loyaltyCardRepository;

    public async Task<LoyaltyCard> CreateAsync(LoyaltyCard newLoyaltyCard) => await _loyaltyCardRepository.AddAsync(newLoyaltyCard);
    public async Task<IEnumerable<LoyaltyCard>> GetAllAsync() => await _loyaltyCardRepository.GetAllAsync();
    public async Task<LoyaltyCard> GetByIdAsync(Guid id, string userEmail) => await _loyaltyCardRepository.GetByIdAsync(id, userEmail);
}