using System;

namespace LoyaltySystem.Domain.Common
{
    /// <summary>
    /// Base class for all domain entities.
    /// </summary>
    /// <typeparam name="TId">The type of the entity's identifier.</typeparam>
    public abstract class Entity<TId> where TId : EntityId
    {
        // For EF Core
        protected Entity() { }
        
        protected Entity(TId id)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }
        
        public TId Id { get; set; }
        
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            
            var other = (Entity<TId>)obj;
            return Id.Equals(other.Id);
        }
        
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        
        public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }
        
        public static bool operator !=(Entity<TId> left, Entity<TId> right)
        {
            return !(left == right);
        }
    }
} 