using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Interfaces;

public interface IAuditService 
{
    Task CreateAuditRecordAsync<T>
    (        
        Guid entityId, 
        InteractionType interactionType, 
        string? interactionId = null
    );
}