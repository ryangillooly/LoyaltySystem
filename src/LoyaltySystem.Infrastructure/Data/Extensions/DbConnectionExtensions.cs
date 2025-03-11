using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace LoyaltySystem.Infrastructure.Data.Extensions
{
    /// <summary>
    /// Extension methods for IDbConnection interfaces.
    /// </summary>
    public static class DbConnectionExtensions
    {
        /// <summary>
        /// Extension method to provide a BeginTransactionAsync method for IDbConnection.
        /// </summary>
        public static async Task<IDbTransaction> BeginTransactionAsync(this IDbConnection connection)
        {
            if (connection is DbConnection dbConnection)
            {
                // If it's a DbConnection (base class for most ADO.NET providers),
                // we can use the async version
                return await dbConnection.BeginTransactionAsync();
            }
            
            // Fallback to synchronous version for non-DbConnection implementations
            return connection.BeginTransaction();
        }
    }
} 