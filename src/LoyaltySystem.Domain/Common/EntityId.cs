namespace LoyaltySystem.Domain.Common;

public abstract class EntityId : IEquatable<EntityId>
{
    private readonly Guid _value;
    public abstract string Prefix { get; }
    public Guid Value => _value;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityId"/> class with the specified non-empty GUID.
    /// </summary>
    /// <param name="value">The GUID value to use as the entity identifier.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is <see cref="Guid.Empty"/>.</exception>
    protected EntityId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Entity ID cannot be empty", nameof(value));
                
        _value = value;
    }
    
    /// <summary>
    /// Returns the string representation of the entity ID as a prefix followed by an underscore and a Base32-encoded GUID.
    /// </summary>
    /// <returns>A string in the format "Prefix_Base32EncodedGuid".</returns>
    public override string ToString()
    {
        string payload = Base32Converter.Encode(_value);
        return $"{Prefix}_{payload}"; // Maintain the underscore separator
    }
        
    /// <summary>
    /// Attempts to parse a prefixed string in the format "prefix_base32payload" into an instance of the specified <typeparamref name="T"/> entity ID type.
    /// </summary>
    /// <typeparam name="T">The entity ID type to parse into, which must derive from <see cref="EntityId"/> and have a parameterless constructor.</typeparam>
    /// <param name="prefixedId">The prefixed string to parse.</param>
    /// <param name="result">When this method returns, contains the parsed entity ID if successful; otherwise, the default value.</param>
    /// <returns><c>true</c> if parsing succeeds and the string matches the expected prefix and payload format; otherwise, <c>false</c>.</returns>
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
    
    /// <summary>
    /// Parses a prefixed string into an instance of the specified <see cref="EntityId"/> type.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="EntityId"/> to parse.</typeparam>
    /// <param name="prefixedId">The prefixed identifier string to parse.</param>
    /// <returns>An instance of <typeparamref name="T"/> representing the parsed identifier.</returns>
    /// <exception cref="FormatException">
    /// Thrown if the input string is null, empty, or does not match the expected format of "Prefix_base32payload".
    /// </exception>
    public static T Parse<T>(string prefixedId) where T : EntityId, new()
    {
        if (TryParse<T>(prefixedId, out var result) && result != null)
            return result;
                
        throw new FormatException($"The input string '{prefixedId}' was not in the correct format for {typeof(T).Name}. Expected format: {new T().Prefix}_base32payload.");
    }
        
    /// <summary>
    /// Determines whether the current EntityId is equal to another EntityId of the same type and value.
    /// </summary>
    /// <param name="other">The EntityId to compare with the current instance.</param>
    /// <returns>True if both EntityIds are of the same type and have the same GUID value; otherwise, false.</returns>
    public bool Equals(EntityId? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return GetType() == other.GetType() && _value.Equals(other._value);
    }
        
    /// <summary>
/// Determines whether the specified object is an <see cref="EntityId"/> and has the same value and prefix as the current instance.
/// </summary>
/// <param name="obj">The object to compare with the current instance.</param>
/// <returns><c>true</c> if the specified object is an <see cref="EntityId"/> with the same value and prefix; otherwise, <c>false</c>.</returns>
public override bool Equals(object? obj) => Equals(obj as EntityId);
    /// <summary>
/// Returns a hash code based on the GUID value and prefix of the entity identifier.
/// </summary>
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