namespace LoyaltySystem.Application.Interfaces;

/// <summary>
/// Generic interface for handling domain events
/// </summary>
/// <typeparam name="TEvent">The type of domain event to handle</typeparam>
public interface IEventHandler<in TEvent>
{
    /// <summary>
    /// Handles the specified domain event
    /// </summary>
    /// <param name="domainEvent">The domain event to handle</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task HandleAsync(TEvent domainEvent);
} 