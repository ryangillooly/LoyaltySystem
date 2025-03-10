using System;
using System.Text.Json;
using System.Threading.Tasks;
using LoyaltySystem.Application.Interfaces;

namespace LoyaltySystem.Infrastructure.Events
{
    /// <summary>
    /// Simple implementation of event publisher that logs events to console.
    /// This is intended for development use only.
    /// </summary>
    public class ConsoleEventPublisher : IEventPublisher
    {
        /// <summary>
        /// Publishes an event by logging it to the console.
        /// </summary>
        public Task PublishAsync<TEvent>(TEvent @event) where TEvent : class
        {
            var eventType = @event.GetType().Name;
            var eventJson = JsonSerializer.Serialize(@event, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"EVENT PUBLISHED: {eventType}");
            Console.ResetColor();
            Console.WriteLine(eventJson);
            Console.WriteLine();
            
            return Task.CompletedTask;
        }
    }
} 