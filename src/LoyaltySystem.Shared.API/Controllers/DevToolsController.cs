using System;
using System.Collections.Generic;
using System.Linq;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Shared.API.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LoyaltySystem.Shared.API.Controllers
{
    /// <summary>
    /// Controller providing development and debugging tools.
    /// Only available in development and testing environments.
    /// </summary>
    [ApiController]
    [Route("api/dev-tools")]
    [ApiExplorerSettings(IgnoreApi = true)] // Hide from Swagger in production
    public class DevToolsController : ControllerBase
    {
        private readonly ILogger<DevToolsController> _logger;
        private readonly bool _isDevelopment;

        public DevToolsController(ILogger<DevToolsController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _isDevelopment = env.IsDevelopment() || env.IsEnvironment("Testing");
        }

        /// <summary>
        /// Get information about a specific entity ID, showing both prefixed and raw formats
        /// </summary>
        [HttpGet("entity-id/info")]
        public IActionResult GetEntityIdInfo([FromQuery] string id)
        {
            if (!_isDevelopment)
                return NotFound();

            if (string.IsNullOrEmpty(id))
                return BadRequest("ID is required");

            try
            {
                // Initialize the utility
                EntityIdUtility.Initialize();

                // Try to determine if this is a prefixed ID
                if (EntityIdUtility.TryGetEntityType(id, out var entityType))
                {
                    // This is a prefixed ID
                    var rawId = EntityIdUtility.GetRawId(id);
                    var entity = Activator.CreateInstance(entityType, new object[] { rawId }) as EntityId;

                    return Ok(new
                    {
                        PrefixedId = id,
                        RawUuid = rawId,
                        EntityType = entityType.Name,
                        Prefix = id.Substring(0, id.IndexOf('_'))
                    });
                }
                else if (Guid.TryParse(id, out var guid))
                {
                    // This is a raw UUID, show all possible entity types it could be
                    var allEntityTypes = from prefix in EntityIdUtility.GetAllPrefixes()
                                         let type = EntityIdUtility.GetTypeForPrefix(prefix)
                                         let instance = Activator.CreateInstance(type, new object[] { guid }) as EntityId
                                         select new
                                         {
                                             EntityType = type.Name,
                                             PrefixedId = instance.ToString(),
                                             Prefix = prefix,
                                             RawUuid = guid
                                         };

                    return Ok(new
                    {
                        RawUuid = guid,
                        PossibleEntityIds = allEntityTypes.ToList()
                    });
                }
                else
                {
                    return BadRequest("Invalid ID format. Provide either a prefixed ID (like cus_123) or a raw UUID.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing entity ID info for {id}", id);
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// List all registered entity ID types and their prefixes
        /// </summary>
        [HttpGet("entity-id/types")]
        public IActionResult ListEntityIdTypes()
        {
            if (!_isDevelopment)
                return NotFound();

            try
            {
                // Initialize the utility
                EntityIdUtility.Initialize();

                var types = new List<object>();

                foreach (var prefix in EntityIdUtility.GetAllPrefixes())
                {
                    var type = EntityIdUtility.GetTypeForPrefix(prefix);
                    types.Add(new
                    {
                        EntityType = type.Name,
                        Prefix = prefix,
                        Example = $"{prefix}_00000000000000000000000000"
                    });
                }

                return Ok(new
                {
                    EntityIdTypes = types,
                    TotalCount = types.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing entity ID types");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Convert between prefixed and raw ID formats
        /// </summary>
        [HttpGet("entity-id/convert")]
        public IActionResult ConvertEntityId([FromQuery] string id, [FromQuery] string targetType = null)
        {
            if (!_isDevelopment)
                return NotFound();

            if (string.IsNullOrEmpty(id))
                return BadRequest("ID is required");

            try
            {
                // Initialize the utility
                EntityIdUtility.Initialize();

                // If this is a raw UUID
                if (Guid.TryParse(id, out var guid))
                {
                    // If we have a target type
                    if (!string.IsNullOrEmpty(targetType))
                    {
                        var foundType = EntityIdUtility.GetTypeForPrefix(targetType);
                        if (foundType == null)
                        {
                            // Try finding by type name
                            foundType = AppDomain.CurrentDomain.GetAssemblies()
                                .SelectMany(a => a.GetTypes())
                                .FirstOrDefault(t => t.IsSubclassOf(typeof(EntityId)) && 
                                                   t.Name.Equals(targetType, StringComparison.OrdinalIgnoreCase));
                        }

                        if (foundType != null)
                        {
                            var entity = Activator.CreateInstance(foundType, new object[] { guid }) as EntityId;
                            return Ok(new
                            {
                                OriginalId = id,
                                ConvertedId = entity.ToString(),
                                EntityType = foundType.Name
                            });
                        }
                        else
                        {
                            return BadRequest($"Unknown entity type: {targetType}");
                        }
                    }
                    else
                    {
                        // Show all possible conversions
                        var conversions = from prefix in EntityIdUtility.GetAllPrefixes()
                                          let type = EntityIdUtility.GetTypeForPrefix(prefix)
                                          let entity = Activator.CreateInstance(type, new object[] { guid }) as EntityId
                                          select new
                                          {
                                              EntityType = type.Name,
                                              ConvertedId = entity.ToString()
                                          };

                        return Ok(new
                        {
                            OriginalId = id,
                            PossibleConversions = conversions.ToList()
                        });
                    }
                }
                else
                {
                    // Assume it's a prefixed ID
                    try
                    {
                        var rawId = EntityIdUtility.GetRawId(id);
                        return Ok(new
                        {
                            OriginalId = id,
                            ConvertedId = rawId.ToString(),
                            Format = "UUID"
                        });
                    }
                    catch
                    {
                        return BadRequest("Invalid ID format. Provide either a prefixed ID (like cus_123) or a raw UUID.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting entity ID: {id}", id);
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
} 