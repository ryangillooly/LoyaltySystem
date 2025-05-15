namespace LoyaltySystem.Domain.Common;

public abstract class Entity<TId> where TId : EntityId
{
    protected Entity(TId id) => Id = id ?? throw new ArgumentNullException(nameof(id));
        
    public TId Id { get; set; }
    public string PrefixedId => Id.ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
            
        var other = (Entity<TId>)obj;
        return Id.Equals(other.Id);
    }
        
    public override int GetHashCode() => Id.GetHashCode();
}