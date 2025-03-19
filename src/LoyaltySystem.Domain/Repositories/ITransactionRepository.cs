using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Repositories
{
    /// <summary>
    /// Repository interface for Transaction entities.
    /// </summary>
    public interface ITransactionRepository
    {
        /// <summary>
        /// Gets a transaction by its ID.
        /// </summary>
        Task<Transaction> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Gets transactions for a specific loyalty card.
        /// </summary>
        Task<IEnumerable<Transaction>> GetByCardIdAsync(Guid cardId);
        
        /// <summary>
        /// Gets transactions for a specific program (across all cards).
        /// </summary>
        Task<IEnumerable<Transaction>> GetByProgramIdAsync(LoyaltyProgramId programId);
        
        /// <summary>
        /// Gets transactions for a specific store.
        /// </summary>
        Task<IEnumerable<Transaction>> GetByStoreIdAsync(Guid storeId);
        
        /// <summary>
        /// Gets transactions by type.
        /// </summary>
        Task<IEnumerable<Transaction>> GetByTypeAsync(TransactionType type, int skip = 0, int take = 100);
        
        /// <summary>
        /// Gets transactions in a date range.
        /// </summary>
        Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int skip = 0, int take = 100);
        
        /// <summary>
        /// Adds a new transaction.
        /// </summary>
        Task AddAsync(Transaction transaction);
    }
} 