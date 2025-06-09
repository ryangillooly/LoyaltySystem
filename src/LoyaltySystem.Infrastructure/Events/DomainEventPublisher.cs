using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using Microsoft.Extensions.Logging;

namespace LoyaltySystem.Infrastructure.Events;

/// <summary>
/// Implementation of domain event publisher that bridges domain events to the application event publisher
/// </summary>
public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<DomainEventPublisher> _logger;

    public DomainEventPublisher(IEventPublisher eventPublisher, ILogger<DomainEventPublisher> logger)
    {
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Publishes all domain events from an entity and clears them
    /// </summary>
    public async Task PublishEventsAsync<TId>(Entity<TId> entity) where TId : EntityId
    {
        if (entity == null) return;

        var domainEvents = entity.DomainEvents.ToList();
        if (!domainEvents.Any()) return;

        _logger.LogInformation("Publishing {EventCount} domain events for entity {EntityType} with ID {EntityId}", 
            domainEvents.Count, typeof(TId).Name, entity.Id);

        foreach (var domainEvent in domainEvents)
        {
            await PublishEventAsync(domainEvent);
        }

        // Clear events after publishing
        entity.ClearDomainEvents();
        
        _logger.LogInformation("Successfully published and cleared {EventCount} domain events for entity {EntityId}", 
            domainEvents.Count, entity.Id);
    }

    /// <summary>
    /// Publishes a single domain event
    /// </summary>
    public async Task PublishEventAsync(IDomainEvent domainEvent)
    {
        if (domainEvent == null) return;

        try
        {
            _logger.LogDebug("Publishing domain event {EventType} with ID {EventId} for aggregate {AggregateType}:{AggregateId}", 
                domainEvent.GetType().Name, 
                domainEvent.EventId,
                domainEvent is DomainEventBase baseEvent ? baseEvent.AggregateType : "Unknown",
                domainEvent is DomainEventBase baseEvent2 ? baseEvent2.AggregateId : "Unknown");

            await _eventPublisher.PublishAsync(domainEvent);
            
            _logger.LogDebug("Successfully published domain event {EventType} with ID {EventId}", 
                domainEvent.GetType().Name, domainEvent.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish domain event {EventType} with ID {EventId}: {Error}", 
                domainEvent.GetType().Name, domainEvent.EventId, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Publishes multiple domain events
    /// </summary>
    public async Task PublishEventsAsync(IEnumerable<IDomainEvent> domainEvents)
    {
        if (domainEvents == null) return;

        var eventsList = domainEvents.ToList();
        if (!eventsList.Any()) return;

        _logger.LogInformation("Publishing {EventCount} domain events", eventsList.Count);

        foreach (var domainEvent in eventsList)
        {
            await PublishEventAsync(domainEvent);
        }

        _logger.LogInformation("Successfully published {EventCount} domain events", eventsList.Count);
    }
} 