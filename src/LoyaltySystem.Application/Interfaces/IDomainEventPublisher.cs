using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Application.Interfaces;

/// <summary>
/// Service for publishing domain events from aggregate roots
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>
    /// Publishes all domain events from an entity and clears them
    /// </summary>
    /// <param name="entity">The entity containing domain events</param>
    Task PublishEventsAsync<TId>(Entity<TId> entity) where TId : EntityId;
    
    /// <summary>
    /// Publishes a single domain event
    /// </summary>
    /// <param name="domainEvent">The domain event to publish</param>
    Task PublishEventAsync(IDomainEvent domainEvent);
    
    /// <summary>
    /// Publishes multiple domain events
    /// </summary>
    /// <param name="domainEvents">The domain events to publish</param>
    Task PublishEventsAsync(IEnumerable<IDomainEvent> domainEvents);
} 