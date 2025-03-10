using System;

namespace LoyaltySystem.Application.Events
{
    /// <summary>
    /// Base class for all domain events.
    /// </summary>
    public abstract class EventBase
    {
        /// <summary>
        /// The unique identifier for the event.
        /// </summary>
        public Guid EventId { get; protected set; }
        
        /// <summary>
        /// When the event occurred.
        /// </summary>
        public DateTime OccurredAt { get; protected set; }
        
        /// <summary>
        /// Creates a new event.
        /// </summary>
        protected EventBase()
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
        }
    }
} 