namespace LoyaltySystem.Domain.Common;

public abstract class Entity<TId> where TId : EntityId
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    protected Entity(TId id) => Id = id ?? throw new ArgumentNullException(nameof(id));
        
    public TId Id { get; set; }
    public string PrefixedId => Id.ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets the domain events that have been raised by this entity
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    /// <summary>
    /// Adds a domain event to be published
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    /// <summary>
    /// Clears all domain events (typically called after publishing)
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
    
    /// <summary>
    /// Marks the entity as updated and records the timestamp
    /// </summary>
    protected void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
        
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