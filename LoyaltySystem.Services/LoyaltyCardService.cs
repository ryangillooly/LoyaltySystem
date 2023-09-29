using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Interfaces;

namespace LoyaltySystem.Services;

public class LoyaltyCardService : ILoyaltyCardService
{
    private readonly ILoyaltyCardRepository _loyaltyCardRepository;
    private readonly IAuditService          _auditService;
    
    public LoyaltyCardService(ILoyaltyCardRepository loyaltyCardRepository, IAuditService auditService) => 
        (_loyaltyCardRepository, _auditService) = (loyaltyCardRepository, auditService);

    public async Task<LoyaltyCard> CreateAsync(LoyaltyCard newLoyaltyCard)
    {
        var auditRecord = new AuditRecord(EntityType.LoyaltyCard, newLoyaltyCard.Id, ActionType.CreateLoyaltyCard);
        
        await _loyaltyCardRepository.CreateAsync(newLoyaltyCard);
        // await _auditService.CreateAuditRecordAsync<LoyaltyCard>(auditRecord); // Look to use Event Handlers for Auditing (event / delegates)

        return newLoyaltyCard;
    }

    public async Task<IEnumerable<LoyaltyCard>> GetAllAsync() => await _loyaltyCardRepository.GetAllAsync();
    public async Task<LoyaltyCard> GetByIdAsync(Guid id, Guid userId) => await _loyaltyCardRepository.GetByIdAsync(id, userId);
}