using System.Data;
using Npgsql;

namespace LoyaltySystem.Infrastructure.Data;

public class PostgresConnection : IDatabaseConnection
{
    private readonly string _connectionString;
    private NpgsqlConnection _connection;
    private NpgsqlTransaction _transaction;
    private bool _disposed;
    
    public PostgresConnection(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }
    
    public async Task<IDbConnection> GetConnectionAsync()
    {
        if (_connection == null)
            _connection = new NpgsqlConnection(_connectionString);
        
        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync();
        
        return _connection;
    }
    
    public async Task<IDbTransaction> BeginTransactionAsync()
    {
        var connection = await GetConnectionAsync();
        _transaction = await _connection.BeginTransactionAsync();
        return _transaction;
    }
    
    public async Task CommitTransactionAsync(IDbTransaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));
            
        if (transaction != _transaction)
            throw new InvalidOperationException("Transaction does not match the active transaction");
        
        await _transaction.CommitAsync();
        _transaction = null;
    }
    
    public async Task RollbackTransactionAsync(IDbTransaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));
            
        if (transaction != _transaction)
            throw new InvalidOperationException("Transaction does not match the active transaction");
        
        await _transaction.RollbackAsync();
        _transaction = null;
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
            _connection?.Dispose();
        }
        
        _transaction = null;
        _connection = null;
        _disposed = true;
    }
}