namespace LoyaltySystem.Domain.Common;

public abstract class Entity<TId> where TId : EntityId
{
    /// <summary>
/// Initializes a new instance of the <see cref="Entity{TId}"/> class with the specified identifier.
/// </summary>
/// <param name="id">The unique identifier for the entity. Must not be null.</param>
/// <exception cref="ArgumentNullException">Thrown if <paramref name="id"/> is null.</exception>
protected Entity(TId id) => Id = id ?? throw new ArgumentNullException(nameof(id));
        
    public TId Id { get; set; }
    public string PrefixedId => Id.ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
    /// <summary>
    /// Determines whether the current entity is equal to another object based on type and identifier.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns>True if the object is of the same type and has the same identifier; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
            
        var other = (Entity<TId>)obj;
        return Id.Equals(other.Id);
    }
        
    /// <summary>
/// Returns the hash code for the entity based on its identifier.
/// </summary>
/// <returns>The hash code of the <c>Id</c> property.</returns>
public override int GetHashCode() => Id.GetHashCode();
}