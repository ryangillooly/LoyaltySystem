using System;
using System.Data;
using System.Threading.Tasks;
using Npgsql;

namespace LoyaltySystem.Infrastructure.Data
{
    /// <summary>
    /// PostgreSQL implementation of the database connection.
    /// </summary>
    public class PostgresConnection : IDatabaseConnection
    {
        private readonly string _connectionString;
        private NpgsqlConnection _connection;
        private NpgsqlTransaction _transaction;
        private bool _disposed;
        
        /// <summary>
        /// Creates a new PostgreSQL connection.
        /// </summary>
        public PostgresConnection(string connectionString)
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
                _connection = new NpgsqlConnection(_connectionString);
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
            _transaction = await _connection.BeginTransactionAsync();
            return _transaction;
        }
        
        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        public async Task CommitTransactionAsync(IDbTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));
                
            if (transaction != _transaction)
                throw new InvalidOperationException("Transaction does not match the active transaction");
            
            await _transaction.CommitAsync();
            _transaction = null;
        }
        
        /// <summary>
        /// Rolls back the current transaction.
        /// </summary>
        public async Task RollbackTransactionAsync(IDbTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));
                
            if (transaction != _transaction)
                throw new InvalidOperationException("Transaction does not match the active transaction");
            
            await _transaction.RollbackAsync();
            _transaction = null;
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