using System;
using System.Text.RegularExpressions;

namespace LoyaltySystem.Domain.Common
{
    /// <summary>
    /// Base class for all entity identifiers in the system.
    /// Supports both internal UUID representation and external prefixed string format.
    /// </summary>
    public abstract class EntityId : IEquatable<EntityId>
    {
        // Internal UUID value
        protected readonly Guid _value;
        
        // External prefix for this type of entity
        public abstract string Prefix { get; }
        
        // Property to access the raw GUID value (for storage)
        public Guid Value => _value;
        
        // Constructor from GUID (internal use)
        protected EntityId(Guid value)
        {
            if (value == Guid.Empty)
                throw new ArgumentException("Entity ID cannot be empty", nameof(value));
                
            _value = value;
        }
        
        // Generate a new random ID
        public static T New<T>() where T : EntityId, new()
        {
            // Use reflection to create a new instance and set its value
            var instance = new T();
            typeof(T).GetField("_value", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(instance, Guid.NewGuid());
            return instance;
        }
        
        // Convert GUID to prefixed Base64 representation
        public override string ToString()
        {
            return $"{Prefix}{EncodeGuid(_value)}";
        }
        
        // Static helper to encode a GUID as URL-safe Base64
        protected static string EncodeGuid(Guid guid)
        {
            return Convert.ToBase64String(guid.ToByteArray())
                .Replace("/", "_")
                .Replace("+", "-")
                .Replace("=", "");
        }
        
        // Static helper to decode a URL-safe Base64 string to a GUID
        protected static Guid DecodeString(string encoded)
        {
            // Ensure proper padding
            string padded = encoded;
            switch (encoded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }
            
            byte[] bytes = Convert.FromBase64String(
                padded.Replace("_", "/").Replace("-", "+"));
                
            return new Guid(bytes);
        }
        
        // Try to parse a prefixed string into the specific ID type
        public static bool TryParse<T>(string prefixedId, out T result) where T : EntityId, new()
        {
            result = new T();
            var prefix = result.Prefix;
            
            if (string.IsNullOrEmpty(prefixedId) || !prefixedId.StartsWith(prefix))
            {
                result = null;
                return false;
            }
            
            try
            {
                // Extract the encoded portion (remove prefix)
                string encodedPart = prefixedId.Substring(prefix.Length);
                
                // Decode to GUID
                Guid guid = DecodeString(encodedPart);
                
                // Set the value on the result instance
                typeof(T).GetField("_value", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    ?.SetValue(result, guid);
                    
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
        
        // Parse a prefixed string into an ID (throws on failure)
        public static T Parse<T>(string prefixedId) where T : EntityId, new()
        {
            if (TryParse<T>(prefixedId, out var result))
                return result;
                
            throw new FormatException($"The input string '{prefixedId}' was not in the correct format for {typeof(T).Name}");
        }
        
        // Equality members
        public bool Equals(EntityId? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return _value.Equals(other._value);
        }
        
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((EntityId)obj);
        }
        
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
        
        public static bool operator ==(EntityId left, EntityId right)
        {
            return Equals(left, right);
        }
        
        public static bool operator !=(EntityId left, EntityId right)
        {
            return !Equals(left, right);
        }
        
        // Implicit conversion to GUID (for use with repositories)
        public static implicit operator Guid(EntityId id) => id._value;
    }
} 