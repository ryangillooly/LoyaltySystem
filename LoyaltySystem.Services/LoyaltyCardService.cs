using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Interfaces;

namespace LoyaltySystem.Services;

public class LoyaltyCardService : ILoyaltyCardService
{
    private readonly IRepository<LoyaltyCard> _loyaltyCardRepository;

    public LoyaltyCardService(IRepository<LoyaltyCard> loyaltyCardRepository)
    {
        _loyaltyCardRepository = loyaltyCardRepository;
    }

    public async Task<IEnumerable<LoyaltyCard>> GetAllAsync()
    {
        return await _loyaltyCardRepository.GetAllAsync();
    }

    public async Task<LoyaltyCard> GetByIdAsync(Guid id)
    {
        return await _loyaltyCardRepository.GetByIdAsync(id);
    }

    // ... (Other methods like CreateAsync, Update, DeleteAsync)
}