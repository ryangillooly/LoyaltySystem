using System.Data;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Specifications;

namespace LoyaltySystem.Domain.Repositories;

/// <summary>
/// Generic repository interface providing common CRUD operations for all entities
/// Reduces code duplication and ensures consistency across repositories
/// </summary>
/// <typeparam name="TEntity">The entity type that inherits from Entity</typeparam>
/// <typeparam name="TId">The entity ID type that inherits from EntityId</typeparam>
public interface IRepository<TEntity, TId> 
    where TEntity : Entity<TId> 
    where TId : EntityId
{
    /// <summary>
    /// Gets an entity by its ID
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>The entity if found, null otherwise</returns>
    Task<TEntity?> GetByIdAsync(TId id);

    /// <summary>
    /// Gets all entities with optional paging
    /// </summary>
    /// <param name="skip">Number of entities to skip</param>
    /// <param name="limit">Maximum number of entities to return</param>
    /// <returns>Collection of entities</returns>
    Task<IEnumerable<TEntity>> GetAllAsync(int skip = 0, int limit = 50);

    /// <summary>
    /// Gets the total count of entities
    /// </summary>
    /// <returns>Total number of entities</returns>
    Task<int> GetTotalCountAsync();

    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <param name="transaction">Optional database transaction</param>
    /// <returns>The added entity</returns>
    Task<TEntity> AddAsync(TEntity entity, IDbTransaction? transaction = null);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <param name="transaction">Optional database transaction</param>
    Task UpdateAsync(TEntity entity, IDbTransaction? transaction = null);

    /// <summary>
    /// Deletes an entity by its ID
    /// </summary>
    /// <param name="id">The ID of the entity to delete</param>
    /// <param name="transaction">Optional database transaction</param>
    Task DeleteAsync(TId id, IDbTransaction? transaction = null);

    /// <summary>
    /// Checks if an entity exists by its ID
    /// </summary>
    /// <param name="id">The entity ID to check</param>
    /// <returns>True if the entity exists, false otherwise</returns>
    Task<bool> ExistsAsync(TId id);

    /// <summary>
    /// Finds entities that match the given specification
    /// </summary>
    /// <param name="specification">The specification to match</param>
    /// <param name="skip">Number of entities to skip</param>
    /// <param name="limit">Maximum number of entities to return</param>
    /// <returns>Collection of entities matching the specification</returns>
    Task<IEnumerable<TEntity>> FindAsync(ISpecification<TEntity> specification, int skip = 0, int limit = 50);

    /// <summary>
    /// Counts entities that match the given specification
    /// </summary>
    /// <param name="specification">The specification to match</param>
    /// <returns>Number of entities matching the specification</returns>
    Task<int> CountAsync(ISpecification<TEntity> specification);

    /// <summary>
    /// Checks if any entity matches the given specification
    /// </summary>
    /// <param name="specification">The specification to match</param>
    /// <returns>True if any entity matches, false otherwise</returns>
    Task<bool> AnyAsync(ISpecification<TEntity> specification);
} 