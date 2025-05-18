using System;
using System.Collections.Generic;
using System.Reflection;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Shared.API.Utilities
{
    /// <summary>
    /// Utility methods for working with entity IDs, including conversion between
    /// prefixed IDs (like cus_123) and raw UUIDs as stored in the database.
    /// </summary>
    public static class EntityIdUtility
    {
        private static readonly Dictionary<string, Type> _prefixToTypeMap = new();
        private static readonly Dictionary<Type, string> _typeToPreixMap = new();
        private static bool _initialized = false;
        private static readonly object _initLock = new();

        /// <summary>
        /// Initialize the prefix mappings by reflecting over entity ID classes
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            lock (_initLock)
            {
                if (_initialized) return;

                // Find all types that inherit from EntityId
                var entityIdTypes = Assembly.GetAssembly(typeof(EntityId))
                    .GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(EntityId)));

                foreach (var type in entityIdTypes)
                {
                    try
                    {
                        // Create an instance to get the prefix
                        var instance = Activator.CreateInstance(type) as EntityId;
                        if (instance != null)
                        {
                            _prefixToTypeMap[instance.Prefix] = type;
                            _typeToPreixMap[type] = instance.Prefix;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log but continue with other types
                        Console.WriteLine($"Error initializing EntityIdUtility for type {type.Name}: {ex.Message}");
                    }
                }
                
                _initialized = true;
            }
        }

        /// <summary>
        /// Gets a list of all registered entity ID prefixes
        /// </summary>
        public static IEnumerable<string> GetAllPrefixes()
        {
            EnsureInitialized();
            return _prefixToTypeMap.Keys;
        }

        /// <summary>
        /// Gets the prefix for a specific EntityId type
        /// </summary>
        public static string GetPrefixForType<T>() where T : EntityId
        {
            EnsureInitialized();
            
            if (_typeToPreixMap.TryGetValue(typeof(T), out var prefix))
            {
                return prefix;
            }
            
            // Try to get prefix from a new instance
            try
            {
                var instance = Activator.CreateInstance<T>();
                var instancePrefix = instance.Prefix;
                _typeToPreixMap[typeof(T)] = instancePrefix;
                _prefixToTypeMap[instancePrefix] = typeof(T);
                return instancePrefix;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the EntityId type for a specific prefix
        /// </summary>
        public static Type GetTypeForPrefix(string prefix)
        {
            EnsureInitialized();
            
            if (string.IsNullOrEmpty(prefix)) return null;
            
            return _prefixToTypeMap.TryGetValue(prefix, out var type) ? type : null;
        }

        /// <summary>
        /// Converts a raw UUID to a prefixed entity ID
        /// </summary>
        public static string GetPrefixedId<T>(Guid rawId) where T : EntityId, new()
        {
            try
            {
                var entity = (T)Activator.CreateInstance(typeof(T), new object[] { rawId });
                return entity.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Cannot create prefixed ID for type {typeof(T).Name} with UUID {rawId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Extracts the raw UUID from a prefixed entity ID
        /// </summary>
        public static Guid GetRawId(string prefixedId)
        {
            if (string.IsNullOrEmpty(prefixedId))
                throw new ArgumentNullException(nameof(prefixedId));

            // Try to extract prefix and ID parts
            int underscoreIndex = prefixedId.IndexOf('_');
            if (underscoreIndex >= 0 && underscoreIndex < prefixedId.Length - 1)
            {
                string idPart = prefixedId.Substring(underscoreIndex + 1);
                
                // Try to parse the ID part as a UUID
                if (Guid.TryParse(idPart, out var guid))
                {
                    return guid;
                }
            }
            
            // If not a prefixed ID, try to parse directly as UUID
            if (Guid.TryParse(prefixedId, out var directGuid))
                return directGuid;
                
            throw new ArgumentException($"Cannot extract UUID from '{prefixedId}'. It is not a valid prefixed ID or UUID.");
        }

        /// <summary>
        /// Creates a strongly-typed EntityId from a prefixed string ID
        /// </summary>
        public static T Parse<T>(string prefixedId) where T : EntityId, new()
        {
            if (string.IsNullOrEmpty(prefixedId))
                throw new ArgumentNullException(nameof(prefixedId));
                
            // Try existing parsing logic first
            if (EntityId.TryParse<T>(prefixedId, out var result))
                return result;
                
            // If that fails, try extracting the ID and creating directly
            var rawId = GetRawId(prefixedId);
            return (T)Activator.CreateInstance(typeof(T), new object[] { rawId });
        }

        /// <summary>
        /// Attempts to determine the entity type from a prefixed ID
        /// </summary>
        public static bool TryGetEntityType(string prefixedId, out Type entityType)
        {
            entityType = null;
            
            if (string.IsNullOrEmpty(prefixedId))
                return false;
                
            int underscoreIndex = prefixedId.IndexOf('_');
            if (underscoreIndex <= 0)
                return false;
                
            string prefix = prefixedId.Substring(0, underscoreIndex);
            entityType = GetTypeForPrefix(prefix);
            
            return entityType != null;
        }

        /// <summary>
        /// Creates an EntityId of the appropriate type from a prefixed ID string
        /// </summary>
        public static EntityId CreateFromPrefixedId(string prefixedId)
        {
            if (!TryGetEntityType(prefixedId, out var entityType))
                throw new ArgumentException($"Cannot determine entity type from prefixed ID: {prefixedId}");
                
            Guid rawId = GetRawId(prefixedId);
            return (EntityId)Activator.CreateInstance(entityType, new object[] { rawId });
        }

        /// <summary>
        /// Displays debug information about an entity ID, showing both prefixed and raw formats
        /// </summary>
        public static string GetDebugString(EntityId entityId)
        {
            if (entityId == null)
                return "null";
                
            return $"{entityId} (Type: {entityId.GetType().Name}, Raw UUID: {entityId.Value})";
        }

        /// <summary>
        /// Creates a formatted string showing the mapping between a prefixed ID and its raw UUID
        /// </summary>
        public static string FormatIdMapping<T>(Guid rawId) where T : EntityId, new()
        {
            string prefixed = GetPrefixedId<T>(rawId);
            return $"{prefixed} ⟷ {rawId}";
        }

        private static void EnsureInitialized()
        {
            if (!_initialized)
                Initialize();
        }
    }
} 