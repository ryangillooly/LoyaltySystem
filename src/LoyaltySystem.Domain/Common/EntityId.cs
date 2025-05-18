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
            return false;
            
        var instanceForPrefix = new T(); 
        var prefix = instanceForPrefix.Prefix;
        var expectedPrefixWithUnderscore = prefix + "_";
            
        if (!prefixedId.StartsWith(expectedPrefixWithUnderscore))
            return false;
            
        string payloadPart = string.Empty; 
        try
        {
            payloadPart = prefixedId.Substring(expectedPrefixWithUnderscore.Length); 
            
            Guid guid = Base32Converter.Decode(payloadPart);
            if (guid == Guid.Empty) 
                return false;
            
            System.Reflection.FieldInfo? valueField = null;
            Type? currentType = typeof(T);
            while (currentType != null && valueField == null)
            {
                valueField = currentType.GetField("_value", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly);
                currentType = currentType.BaseType;
            }

            if (valueField == null)
                throw new InvalidOperationException($"Could not find the internal '_value' field on type {typeof(T).Name} or its base classes.");
                 
            result = instanceForPrefix;
            valueField.SetValue(result, guid);
                
            return true;
        }
        catch (FormatException) 
        {
            Console.WriteLine($"Base32Converter.Decode threw FormatException for payload: {payloadPart}"); // Added logging
            return false;
        }
        catch (ArgumentException ex) 
        {
            Console.WriteLine($"Base32Converter.Decode threw ArgumentException for payload '{payloadPart}': {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing EntityId: {ex.Message}"); // Basic logging
            return false;
        }
    }
    
    public static T Parse<T>(string prefixedId) where T : EntityId, new()
    {
        if (TryParse<T>(prefixedId, out var result) && result != null)
            return result;
                
        throw new FormatException($"The input string '{prefixedId}' was not in the correct format for {typeof(T).Name}. Expected format: {new T().Prefix}_base32payload.");
    }
        
    public bool Equals(EntityId? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return GetType() == other.GetType() && _value.Equals(other._value);
    }
        
    public override bool Equals(object? obj) => Equals(obj as EntityId);
    public override int GetHashCode() => HashCode.Combine(_value, Prefix);
    public static bool operator ==(EntityId? left, EntityId? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }
        
    public static bool operator !=(EntityId? left, EntityId? right) =>
        !(left == right);
    
    public static implicit operator Guid(EntityId id) =>
        id?._value ?? 
        throw new ArgumentNullException(nameof(id), "Cannot convert null EntityId to Guid.");
}