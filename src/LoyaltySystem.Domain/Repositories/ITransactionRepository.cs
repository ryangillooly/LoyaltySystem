using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Repositories;

public interface ITransactionRepository
{
    Task<Transaction> GetByIdAsync(Guid id);
    Task<IEnumerable<Transaction>> GetByCardIdAsync(Guid cardId);
    Task<IEnumerable<Transaction>> GetByProgramIdAsync(LoyaltyProgramId programId);
    Task<IEnumerable<Transaction>> GetByStoreIdAsync(Guid storeId);
    Task<IEnumerable<Transaction>> GetByTypeAsync(TransactionType type, int skip = 0, int take = 100);
    Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int skip = 0, int take = 100);
    Task AddAsync(Transaction transaction);
}