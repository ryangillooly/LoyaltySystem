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
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);
    Task ExecuteInTransactionAsync(Func<Task> operation);
}