using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Infrastructure.Repositories;

namespace LoyaltySystem.Infrastructure.Data
{
    /// <summary>
    /// Implementation of the unit of work pattern for coordinating transactions across repositories.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDatabaseConnection _dbConnection;
        private IDbTransaction _transaction;
        private bool _disposed;
        
        private IBrandRepository _brandRepository;
        private ILoyaltyProgramRepository _loyaltyProgramRepository;
        private ILoyaltyCardRepository _loyaltyCardRepository;
        private ITransactionRepository _transactionRepository;
        private ICustomerRepository _customerRepository;
        private IUserRepository _userRepository;
        private IStoreRepository _storeRepository;
        
        public IDbTransaction CurrentTransaction => _transaction;
        
        public IBrandRepository BrandRepository => 
            _brandRepository ??= new BrandRepository(_dbConnection);
        
        public ILoyaltyProgramRepository LoyaltyProgramRepository => 
            _loyaltyProgramRepository ??= new LoyaltyProgramRepository(_dbConnection);

        public ILoyaltyCardRepository LoyaltyCardRepository => 
            _loyaltyCardRepository ??= new LoyaltyCardRepository(_dbConnection);
        
        public ITransactionRepository TransactionRepository => 
            _transactionRepository ??= new TransactionRepository(_dbConnection);
     
        public UnitOfWork(IDatabaseConnection dbConnection) =>
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        
        public async Task BeginTransactionAsync() =>
            _transaction = await _dbConnection.BeginTransactionAsync();
        
        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("No active transaction to commit");
                
            try
            {
                await _dbConnection.CommitTransactionAsync(_transaction);
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }
        
        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("No active transaction to roll back");
                
            try
            {
                await _dbConnection.RollbackTransactionAsync(_transaction);
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }
        
        /// <summary>
        /// Saves changes to the database.
        /// With Dapper, there's no change tracking, so this is just a placeholder.
        /// Actual persistence happens when the queries are executed.
        /// </summary>
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // In a Dapper world, we don't have a SaveChanges concept like EF
            // Changes are persisted when the SQL commands are executed
            return Task.FromResult(0);
        }
        
        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
        {
            ArgumentNullException.ThrowIfNull(operation);

            try
            {
                await BeginTransactionAsync();
                var result = await operation();
                await CommitTransactionAsync();
                return result;
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
        }
        
        public async Task ExecuteInTransactionAsync(Func<Task> operation)
        {
            ArgumentNullException.ThrowIfNull(operation);

            try
            {
                await BeginTransactionAsync();
                await operation();
                await CommitTransactionAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) 
                return;
                
            if (disposing)
            {
                _transaction?.Dispose();
                _transaction = null;
            }
            
            _disposed = true;
        }
    }
} 