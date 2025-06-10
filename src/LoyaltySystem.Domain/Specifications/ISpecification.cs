using System.Linq.Expressions;

namespace LoyaltySystem.Domain.Specifications;

/// <summary>
/// Specification pattern interface for encapsulating complex query logic
/// Allows for composable and reusable query conditions
/// </summary>
/// <typeparam name="T">The entity type to query</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// The criteria expression that defines the specification
    /// </summary>
    Expression<Func<T, bool>> Criteria { get; }

    /// <summary>
    /// Include expressions for eager loading related entities
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Include expressions for string-based includes (for complex navigation properties)
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Order by expressions for sorting
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Order by descending expressions for sorting
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Group by expression for grouping results
    /// </summary>
    Expression<Func<T, object>>? GroupBy { get; }

    /// <summary>
    /// Number of records to skip (for paging)
    /// </summary>
    int Skip { get; }

    /// <summary>
    /// Number of records to take (for paging)
    /// </summary>
    int Take { get; }

    /// <summary>
    /// Whether paging is enabled
    /// </summary>
    bool IsPagingEnabled { get; }

    /// <summary>
    /// Combines this specification with another using AND logic
    /// </summary>
    /// <param name="specification">The specification to combine with</param>
    /// <returns>A new specification representing the AND combination</returns>
    ISpecification<T> And(ISpecification<T> specification);

    /// <summary>
    /// Combines this specification with another using OR logic
    /// </summary>
    /// <param name="specification">The specification to combine with</param>
    /// <returns>A new specification representing the OR combination</returns>
    ISpecification<T> Or(ISpecification<T> specification);

    /// <summary>
    /// Negates this specification
    /// </summary>
    /// <returns>A new specification representing the NOT of this specification</returns>
    ISpecification<T> Not();

    /// <summary>
    /// Checks if the given entity satisfies this specification
    /// </summary>
    /// <param name="entity">The entity to check</param>
    /// <returns>True if the entity satisfies the specification, false otherwise</returns>
    bool IsSatisfiedBy(T entity);
} 