using System.Linq.Expressions;

namespace LoyaltySystem.Domain.Specifications;

/// <summary>
/// Base implementation of the specification pattern
/// Provides common functionality for building complex queries
/// </summary>
/// <typeparam name="T">The entity type to query</typeparam>
public abstract class BaseSpecification<T> : ISpecification<T>
{
    /// <summary>
    /// Initializes a new instance with the given criteria
    /// </summary>
    /// <param name="criteria">The criteria expression</param>
    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
        Includes = new List<Expression<Func<T, object>>>();
        IncludeStrings = new List<string>();
    }

    /// <summary>
    /// Initializes a new instance without criteria (for derived classes)
    /// </summary>
    protected BaseSpecification()
    {
        Includes = new List<Expression<Func<T, object>>>();
        IncludeStrings = new List<string>();
    }

    public virtual Expression<Func<T, bool>> Criteria { get; protected set; } = null!;
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public Expression<Func<T, object>>? GroupBy { get; private set; }
    public int Skip { get; private set; }
    public int Take { get; private set; }
    public bool IsPagingEnabled { get; private set; }

    /// <summary>
    /// Adds an include expression for eager loading
    /// </summary>
    /// <param name="includeExpression">The include expression</param>
    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    /// <summary>
    /// Adds a string-based include for complex navigation properties
    /// </summary>
    /// <param name="includeString">The include string</param>
    protected virtual void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    /// <summary>
    /// Sets the order by expression
    /// </summary>
    /// <param name="orderByExpression">The order by expression</param>
    protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    /// <summary>
    /// Sets the order by descending expression
    /// </summary>
    /// <param name="orderByDescExpression">The order by descending expression</param>
    protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
    }

    /// <summary>
    /// Sets the group by expression
    /// </summary>
    /// <param name="groupByExpression">The group by expression</param>
    protected virtual void ApplyGroupBy(Expression<Func<T, object>> groupByExpression)
    {
        GroupBy = groupByExpression;
    }

    /// <summary>
    /// Applies paging to the specification
    /// </summary>
    /// <param name="skip">Number of records to skip</param>
    /// <param name="take">Number of records to take</param>
    protected virtual void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    /// <summary>
    /// Combines this specification with another using AND logic
    /// </summary>
    public virtual ISpecification<T> And(ISpecification<T> specification)
    {
        return new AndSpecification<T>(this, specification);
    }

    /// <summary>
    /// Combines this specification with another using OR logic
    /// </summary>
    public virtual ISpecification<T> Or(ISpecification<T> specification)
    {
        return new OrSpecification<T>(this, specification);
    }

    /// <summary>
    /// Negates this specification
    /// </summary>
    public virtual ISpecification<T> Not()
    {
        return new NotSpecification<T>(this);
    }

    /// <summary>
    /// Checks if the given entity satisfies this specification
    /// </summary>
    public virtual bool IsSatisfiedBy(T entity)
    {
        return Criteria?.Compile()(entity) ?? true;
    }
}

/// <summary>
/// Specification that combines two specifications with AND logic
/// </summary>
internal class AndSpecification<T> : BaseSpecification<T>
{
    public AndSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        var parameter = Expression.Parameter(typeof(T));
        var leftExpression = ReplaceParameter(left.Criteria, parameter);
        var rightExpression = ReplaceParameter(right.Criteria, parameter);
        
        Criteria = Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(leftExpression, rightExpression), parameter);
    }

    private static Expression ReplaceParameter(Expression expression, ParameterExpression parameter)
    {
        return new ParameterReplacer(parameter).Visit(expression);
    }
}

/// <summary>
/// Specification that combines two specifications with OR logic
/// </summary>
internal class OrSpecification<T> : BaseSpecification<T>
{
    public OrSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        var parameter = Expression.Parameter(typeof(T));
        var leftExpression = ReplaceParameter(left.Criteria, parameter);
        var rightExpression = ReplaceParameter(right.Criteria, parameter);
        
        Criteria = Expression.Lambda<Func<T, bool>>(
            Expression.OrElse(leftExpression, rightExpression), parameter);
    }

    private static Expression ReplaceParameter(Expression expression, ParameterExpression parameter)
    {
        return new ParameterReplacer(parameter).Visit(expression);
    }
}

/// <summary>
/// Specification that negates another specification
/// </summary>
internal class NotSpecification<T> : BaseSpecification<T>
{
    public NotSpecification(ISpecification<T> specification)
    {
        var parameter = Expression.Parameter(typeof(T));
        var expression = ReplaceParameter(specification.Criteria, parameter);
        
        Criteria = Expression.Lambda<Func<T, bool>>(
            Expression.Not(expression), parameter);
    }

    private static Expression ReplaceParameter(Expression expression, ParameterExpression parameter)
    {
        return new ParameterReplacer(parameter).Visit(expression);
    }
}

/// <summary>
/// Expression visitor for replacing parameters in expressions
/// </summary>
internal class ParameterReplacer : ExpressionVisitor
{
    private readonly ParameterExpression _parameter;

    public ParameterReplacer(ParameterExpression parameter)
    {
        _parameter = parameter;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return base.VisitParameter(_parameter);
    }
} 