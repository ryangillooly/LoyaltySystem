using System;
using System.Data;
using System.Threading.Tasks;

namespace LoyaltySystem.Infrastructure.Data
{
    /// <summary>
    /// Interface for database connection management.
    /// </summary>
    public interface IDatabaseConnection : IDisposable
    {
        /// <summary>
        /// Gets an open database connection.
        /// </summary>
        Task<IDbConnection> GetConnectionAsync();
        
        /// <summary>
        /// Begins a new transaction.
        /// </summary>
        Task<IDbTransaction> BeginTransactionAsync();
        
        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        Task CommitTransactionAsync(IDbTransaction transaction);
        
        /// <summary>
        /// Rolls back the current transaction.
        /// </summary>
        Task RollbackTransactionAsync(IDbTransaction transaction);
    }
} 