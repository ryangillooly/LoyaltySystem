using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace LoyaltySystem.Infrastructure.Data
{
    /// <summary>
    /// SQL Server implementation of the database connection.
    /// </summary>
    public class SqlServerConnection : IDatabaseConnection
    {
        private readonly string _connectionString;
        private SqlConnection _connection;
        private SqlTransaction _transaction;
        private bool _disposed;
        
        /// <summary>
        /// Creates a new SQL Server connection.
        /// </summary>
        public SqlServerConnection(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }
        
        /// <summary>
        /// Gets an open connection to the database.
        /// </summary>
        public async Task<IDbConnection> GetConnectionAsync()
        {
            if (_connection == null)
            {
                _connection = new SqlConnection(_connectionString);
            }
            
            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync();
            }
            
            return _connection;
        }
        
        /// <summary>
        /// Begins a new transaction.
        /// </summary>
        public async Task<IDbTransaction> BeginTransactionAsync()
        {
            var connection = await GetConnectionAsync();
            _transaction = (SqlTransaction)await _connection.BeginTransactionAsync();
            return _transaction;
        }
        
        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        public Task CommitTransactionAsync(IDbTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));
                
            if (transaction != _transaction)
                throw new InvalidOperationException("Transaction does not match the active transaction");
                
            transaction.Commit();
            _transaction = null;
            
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Rolls back the current transaction.
        /// </summary>
        public Task RollbackTransactionAsync(IDbTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));
                
            if (transaction != _transaction)
                throw new InvalidOperationException("Transaction does not match the active transaction");
                
            transaction.Rollback();
            _transaction = null;
            
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Disposes the connection and transaction if open.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Disposes the connection and transaction if open.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
                
            if (disposing)
            {
                _transaction?.Dispose();
                _connection?.Dispose();
            }
            
            _transaction = null;
            _connection = null;
            _disposed = true;
        }
    }
} 