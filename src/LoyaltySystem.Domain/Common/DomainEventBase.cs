namespace LoyaltySystem.Domain.Common;

/// <summary>
/// Base class for all domain events providing common implementation
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public virtual int Version { get; init; } = 1;
    
    /// <summary>
    /// The aggregate root ID that generated this event
    /// </summary>
    public abstract Guid AggregateId { get; init; }
    
    /// <summary>
    /// The type of aggregate that generated this event
    /// </summary>
    public abstract string AggregateType { get; init; }
} 