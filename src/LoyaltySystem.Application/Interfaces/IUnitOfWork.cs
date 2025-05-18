using System.Data;
using LoyaltySystem.Domain.Repositories;

namespace LoyaltySystem.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IDbTransaction CurrentTransaction { get; }
    IBrandRepository BrandRepository { get; }
    ILoyaltyProgramRepository LoyaltyProgramRepository { get; }
    ILoyaltyCardRepository LoyaltyCardRepository { get; }
    ITransactionRepository TransactionRepository { get; }
    /// <summary>
/// Asynchronously persists all pending changes to the data store.
/// </summary>
/// <param name="cancellationToken">Token to observe while waiting for the operation to complete.</param>
/// <returns>The number of state entries written to the data store.</returns>
Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    /// <summary>
/// Begins a new database transaction asynchronously.
/// </summary>
Task BeginTransactionAsync();
    /// <summary>
/// Commits the current database transaction asynchronously.
/// </summary>
Task CommitTransactionAsync();
    /// <summary>
/// Asynchronously rolls back the current database transaction, reverting any uncommitted changes.
/// </summary>
Task RollbackTransactionAsync();
    /// <summary>
/// Executes the specified asynchronous operation within a database transaction and returns its result.
/// </summary>
/// <typeparam name="T">The type of the result returned by the operation.</typeparam>
/// <param name="operation">An asynchronous function to execute within the transaction.</param>
/// <returns>The result of the operation.</returns>
Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);
    /// <summary>
/// Executes the specified asynchronous operation within a database transaction.
/// </summary>
/// <param name="operation">The asynchronous operation to execute transactionally.</param>
/// <returns>A task representing the asynchronous operation.</returns>
Task ExecuteInTransactionAsync(Func<Task> operation);
}