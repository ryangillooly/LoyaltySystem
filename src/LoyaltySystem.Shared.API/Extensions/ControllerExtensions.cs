using System;
using System.Text;
using LoyaltySystem.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoyaltySystem.Shared.API.Extensions
{
    /// <summary>
    /// Extension methods for controllers to handle common operations
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Tries to parse a string as a prefixed entity ID
        /// </summary>
        /// <typeparam name="TEntityId">The entity ID type to parse into</typeparam>
        /// <param name="controller">The controller instance</param>
        /// <param name="idString">The ID string to parse</param>
        /// <param name="entityId">The resulting entity ID if successful</param>
        /// <param name="logger">Optional logger to record parsing information</param>
        /// <param name="idTypeName">Optional type name for error messages</param>
        /// <returns>A BadRequest result if parsing fails, null if successful</returns>
        public static ActionResult TryParseId<TEntityId>(
            this ControllerBase controller,
            string idString,
            out TEntityId entityId,
            ILogger logger = null,
            string idTypeName = null) where TEntityId : EntityId, new()
        {
            // Use the specific ID type name if provided, otherwise use the generic type name
            string typeName = idTypeName ?? typeof(TEntityId).Name;
            
            // Only accept prefixed IDs
            if (EntityId.TryParse<TEntityId>(idString, out entityId))
            {
                logger?.LogInformation("Successfully parsed prefixed {TypeName}: {Id}", typeName, entityId);
                return null;
            }
            
            // If we got here, parsing failed
            logger?.LogWarning("Invalid {TypeName} format: {InvalidId}", typeName, idString);
            entityId = default;
            
            // Create a temporary instance to get the prefix
            var tempInstance = new TEntityId();
            var prefix = tempInstance.Prefix;
            
            return controller.BadRequest(new { 
                message = $"Invalid {typeName} format. Please provide a prefixed ID (format: {prefix}xxx)." 
            });
        }
        
        /// <summary>
        /// Generates a prefixed entity ID string from a GUID (internal use only)
        /// </summary>
        public static string GeneratePrefixedId<TEntityId>(Guid id) where TEntityId : EntityId, new()
        {
            var entityId = (TEntityId)Activator.CreateInstance(typeof(TEntityId), new object[] { id });
            return entityId.ToString();
        }
    }
}