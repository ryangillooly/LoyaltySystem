using System;
using System.Collections.Generic;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Shared.API.Utilities
{
    /// <summary>
    /// Utility class for converting between EntityId objects and their string representations
    /// </summary>
    public static class IdConverter
    {
        /// <summary>
        /// Converts an EntityId to its string representation
        /// </summary>
        public static string ToString<T>(T id) where T : EntityId
        {
            return id?.ToString();
        }
        
        /// <summary>
        /// Converts a collection of EntityIds to their string representations
        /// </summary>
        public static List<string> ToStringList<T>(IEnumerable<T> ids) where T : EntityId
        {
            if (ids == null) return new List<string>();
            
            var result = new List<string>();
            foreach (var id in ids)
            {
                result.Add(id.ToString());
            }
            
            return result;
        }
        
        /// <summary>
        /// Tries to parse a string into an EntityId
        /// </summary>
        public static bool TryParse<T>(string idString, out T id) where T : EntityId, new()
        {
            return EntityId.TryParse<T>(idString, out id);
        }
        
        /// <summary>
        /// Parses a string into an EntityId (throws exception on failure)
        /// </summary>
        public static T Parse<T>(string idString) where T : EntityId, new()
        {
            return EntityId.Parse<T>(idString);
        }
        
        /// <summary>
        /// Converts a string representation to an EntityId, or null if the string is null or empty
        /// </summary>
        public static T ToEntityId<T>(string idString) where T : EntityId, new()
        {
            if (string.IsNullOrEmpty(idString)) return null;
            
            if (EntityId.TryParse<T>(idString, out var result))
            {
                return result;
            }
            
            throw new FormatException($"The input string '{idString}' was not in the correct format for {typeof(T).Name}");
        }
        
        /// <summary>
        /// Converts a collection of string representations to EntityId objects
        /// </summary>
        public static List<T> ToEntityIdList<T>(IEnumerable<string> idStrings) where T : EntityId, new()
        {
            if (idStrings == null) return new List<T>();
            
            var result = new List<T>();
            foreach (var idString in idStrings)
            {
                if (string.IsNullOrEmpty(idString)) continue;
                
                if (EntityId.TryParse<T>(idString, out var id))
                {
                    result.Add(id);
                }
            }
            
            return result;
        }
    }
} 