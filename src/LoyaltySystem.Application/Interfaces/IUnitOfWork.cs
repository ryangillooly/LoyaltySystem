using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using LoyaltySystem.Domain.Repositories;

namespace LoyaltySystem.Application.Interfaces
{
    /// <summary>
    /// Coordinates operations across multiple repositories in a single transaction.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Gets the current active transaction, if any.
        /// </summary>
        IDbTransaction CurrentTransaction { get; }
        
        /// <summary>
        /// Gets the brand repository.
        /// </summary>
        IBrandRepository BrandRepository { get; }
        
        /// <summary>
        /// Gets the loyalty program repository.
        /// </summary>
        ILoyaltyProgramRepository LoyaltyProgramRepository { get; }
        
        /// <summary>
        /// Gets the loyalty card repository.
        /// </summary>
        ILoyaltyCardRepository LoyaltyCardRepository { get; }
        
        /// <summary>
        /// Gets the transaction repository
        /// </summary>
        ITransactionRepository TransactionRepository { get; }
        
        /// <summary>
        /// Saves all changes made in this unit of work to the database.
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Begins a transaction.
        /// </summary>
        Task BeginTransactionAsync();
        
        /// <summary>
        /// Commits the transaction.
        /// </summary>
        Task CommitTransactionAsync();
        
        /// <summary>
        /// Rolls back the transaction.
        /// </summary>
        Task RollbackTransactionAsync();

        /// <summary>
        /// Executes an operation within a transaction and returns a result.
        /// </summary>
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);
        
        /// <summary>
        /// Executes an operation within a transaction.
        /// </summary>
        Task ExecuteInTransactionAsync(Func<Task> operation);
    }
} 