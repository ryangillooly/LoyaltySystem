namespace LoyaltySystem.Domain.Common;

public abstract class EntityId : IEquatable<EntityId>
{
    private readonly Guid _value;
    public abstract string Prefix { get; }
    public Guid Value => _value;
    
    protected EntityId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Entity ID cannot be empty", nameof(value));
                
        _value = value;
    }
        
    public static T New<T>() where T : EntityId, new()
    {
        var instance = new T();
        var valueField = typeof(T).GetField("_value", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        
        if (valueField == null)
            throw new InvalidOperationException($"Could not find the internal '_value' field on type {typeof(T).Name}");
        
        valueField.SetValue(instance, Guid.NewGuid());
        return instance;
    }
        
    public override string ToString()
    {
        string payload = Base32Converter.Encode(_value);
        return $"{Prefix}_{payload}"; // Maintain the underscore separator
    }
        
    // Try to parse a prefixed string ("prefix_base32payload") into the specific ID type
    public static bool TryParse<T>(string? prefixedId, out T? result) where T : EntityId, new()
    {
        result = default;
        if (string.IsNullOrEmpty(prefixedId))
        {
            return false;
        }
            
        var instanceForPrefix = new T(); // Create instance to get prefix
        var prefix = instanceForPrefix.Prefix;
        var expectedPrefixWithUnderscore = prefix + "_";
            
        if (!prefixedId.StartsWith(expectedPrefixWithUnderscore))
        {
            return false;
        }
            
        string payloadPart = string.Empty; // Declare outside try
        try
        {
            // Extract the payload portion
            payloadPart = prefixedId.Substring(expectedPrefixWithUnderscore.Length); // Assign inside try
                
            // Decode the Base32 payload to GUID
            Guid guid = Base32Converter.Decode(payloadPart);
            if (guid == Guid.Empty) // Should not happen if decode is correct
            {
                return false;
            }
                
            // Set the value on the result instance - Search hierarchy for the field
            System.Reflection.FieldInfo? valueField = null;
            Type? currentType = typeof(T);
            while (currentType != null && valueField == null)
            {
                valueField = currentType.GetField("_value", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly);
                currentType = currentType.BaseType;
            }

            if (valueField == null)
                throw new InvalidOperationException($"Could not find the internal '_value' field on type {typeof(T).Name} or its base classes.");
                 
            result = instanceForPrefix; // Reuse instance
            valueField.SetValue(result, guid);
                
            return true;
        }
        catch (FormatException) // Catch errors from Base32Converter.Decode
        {
            Console.WriteLine($"Base32Converter.Decode threw FormatException for payload: {payloadPart}"); // Added logging
            return false;
        }
        catch (ArgumentException ex) // Catch ArgumentExceptions from Base32Converter
        {
            // Log the specific message from the ArgumentException
            Console.WriteLine($"Base32Converter.Decode threw ArgumentException for payload '{payloadPart}': {ex.Message}"); 
            // The secondary error below might be confusing, so let's remove it or make it clearer
            // Console.WriteLine($"Error parsing EntityId (Base32): {ex.Message}"); // Original secondary log
            return false;
        }
        catch (Exception ex) // Catch other potential errors (like reflection)
        {
            // Log error maybe? For now, just fail parsing.
            Console.WriteLine($"Error parsing EntityId: {ex.Message}"); // Basic logging
            return false;
        }
    }
        
    // Parse a prefixed string into an ID (throws on failure)
    public static T Parse<T>(string prefixedId) where T : EntityId, new()
    {
        if (TryParse<T>(prefixedId, out var result) && result != null)
            return result;
                
        throw new FormatException($"The input string '{prefixedId}' was not in the correct format for {typeof(T).Name}. Expected format: {new T().Prefix}_base32payload.");
    }
        
    // Equality members
    public bool Equals(EntityId? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        // Also check type compatibility, though Prefix check might suffice
        return GetType() == other.GetType() && _value.Equals(other._value);
    }
        
    public override bool Equals(object? obj)
    {
        return Equals(obj as EntityId);
    }
        
    public override int GetHashCode()
    {
        // Combine hash codes of value and potentially prefix for better distribution
        return HashCode.Combine(_value, Prefix);
    }
        
    public static bool operator ==(EntityId? left, EntityId? right)
    {
        if (left is null)
            return right is null;
        return left.Equals(right);
    }
        
    public static bool operator !=(EntityId? left, EntityId? right)
    {
        return !(left == right);
    }
        
    // Implicit conversion to GUID (for use with repositories)
    public static implicit operator Guid(EntityId id)
    {
        if (id == null)
            throw new ArgumentNullException(nameof(id), "Cannot convert null EntityId to Guid.");
        return id._value;
    }
        
    // Explicit conversion from Guid might be safer if needed
    // public static explicit operator EntityId<T>(Guid value) => new T { _value = value }; // Requires generic type
}