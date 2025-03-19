using System.Threading.Tasks;

namespace LoyaltySystem.Application.Interfaces
{
    /// <summary>
    /// Publishes domain events to the event bus.
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Publishes an event to the event bus.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to publish.</typeparam>
        /// <param name="event">The event to publish.</param>
        Task PublishAsync<TEvent>(TEvent @event) where TEvent : class;
    }
} 