namespace LoyaltySystem.Domain.Common;

/// <summary>
/// Base interface for all domain events
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier for this event instance
    /// </summary>
    Guid EventId { get; }
    
    /// <summary>
    /// When this event occurred
    /// </summary>
    DateTime OccurredAt { get; }
    
    /// <summary>
    /// Version of the event schema (for future compatibility)
    /// </summary>
    int Version { get; }
} 