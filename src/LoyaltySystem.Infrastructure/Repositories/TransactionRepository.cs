using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Infrastructure.Data;

namespace LoyaltySystem.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Transaction entities using Dapper.
    /// </summary>
    public class TransactionRepository : ITransactionRepository
    {
        private readonly IDatabaseConnection _dbConnection;

        public TransactionRepository(IDatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        public async Task<Transaction> GetByIdAsync(Guid id)
        {
            const string sql = @"
                SELECT 
                    id AS Id,
                    card_id AS CardId,
                    type::int AS Type,
                    reward_id AS RewardId,
                    quantity AS Quantity,
                    points_amount AS PointsAmount,
                    transaction_amount AS TransactionAmount,
                    store_id AS StoreId,
                    staff_id AS StaffId,
                    pos_transaction_id AS PosTransactionId,
                    timestamp AS Timestamp,
                    created_at AS CreatedAt,
                    metadata AS Metadata
                FROM transactions
                WHERE id = @Id";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.QuerySingleOrDefaultAsync<Transaction>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Transaction>> GetByCardIdAsync(Guid cardId)
        {
            const string sql = @"
                SELECT 
                    id AS Id,
                    card_id AS CardId,
                    type::int AS Type,
                    reward_id AS RewardId,
                    quantity AS Quantity,
                    points_amount AS PointsAmount,
                    transaction_amount AS TransactionAmount,
                    store_id AS StoreId,
                    staff_id AS StaffId,
                    pos_transaction_id AS PosTransactionId,
                    timestamp AS Timestamp,
                    created_at AS CreatedAt,
                    metadata AS Metadata
                FROM transactions
                WHERE card_id = @CardId
                ORDER BY timestamp DESC";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.QueryAsync<Transaction>(sql, new { CardId = cardId });
        }

        public async Task<IEnumerable<Transaction>> GetByProgramIdAsync(LoyaltyProgramId programId)
        {
            const string sql = @"
                SELECT 
                    t.id AS Id,
                    t.card_id AS CardId,
                    t.type::int AS Type,
                    t.reward_id AS RewardId,
                    t.quantity AS Quantity,
                    t.points_amount AS PointsAmount,
                    t.transaction_amount AS TransactionAmount,
                    t.store_id AS StoreId,
                    t.staff_id AS StaffId,
                    t.pos_transaction_id AS PosTransactionId,
                    t.timestamp AS Timestamp,
                    t.created_at AS CreatedAt,
                    t.metadata AS Metadata
                FROM transactions t
                JOIN loyalty_cards c ON t.card_id = c.id
                WHERE c.program_id = @ProgramId
                ORDER BY t.timestamp DESC";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.QueryAsync<Transaction>(sql, new { ProgramId = programId.Value });
        }

        public async Task<IEnumerable<Transaction>> GetByStoreIdAsync(Guid storeId)
        {
            const string sql = @"
                SELECT 
                    id AS Id,
                    card_id AS CardId,
                    type::int AS Type,
                    reward_id AS RewardId,
                    quantity AS Quantity,
                    points_amount AS PointsAmount,
                    transaction_amount AS TransactionAmount,
                    store_id AS StoreId,
                    staff_id AS StaffId,
                    pos_transaction_id AS PosTransactionId,
                    timestamp AS Timestamp,
                    created_at AS CreatedAt,
                    metadata AS Metadata
                FROM transactions
                WHERE store_id = @StoreId
                ORDER BY timestamp DESC";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.QueryAsync<Transaction>(sql, new { StoreId = storeId });
        }

        public async Task<IEnumerable<Transaction>> GetByTypeAsync(TransactionType type, int skip = 0, int take = 100)
        {
            const string sql = @"
                SELECT 
                    id AS Id,
                    card_id AS CardId,
                    type::int AS Type,
                    reward_id AS RewardId,
                    quantity AS Quantity,
                    points_amount AS PointsAmount,
                    transaction_amount AS TransactionAmount,
                    store_id AS StoreId,
                    staff_id AS StaffId,
                    pos_transaction_id AS PosTransactionId,
                    timestamp AS Timestamp,
                    created_at AS CreatedAt,
                    metadata AS Metadata
                FROM transactions
                WHERE type = @Type::transaction_type
                ORDER BY timestamp DESC
                LIMIT @Take OFFSET @Skip";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.QueryAsync<Transaction>(sql, new { Type = type.ToString(), Skip = skip, Take = take });
        }

        public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int skip = 0, int take = 100)
        {
            const string sql = @"
                SELECT 
                    id AS Id,
                    card_id AS CardId,
                    type::int AS Type,
                    reward_id AS RewardId,
                    quantity AS Quantity,
                    points_amount AS PointsAmount,
                    transaction_amount AS TransactionAmount,
                    store_id AS StoreId,
                    staff_id AS StaffId,
                    pos_transaction_id AS PosTransactionId,
                    timestamp AS Timestamp,
                    created_at AS CreatedAt,
                    metadata AS Metadata
                FROM transactions
                WHERE timestamp BETWEEN @StartDate AND @EndDate
                ORDER BY timestamp DESC
                LIMIT @Take OFFSET @Skip";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.QueryAsync<Transaction>(sql, new { StartDate = startDate, EndDate = endDate, Skip = skip, Take = take });
        }

        public async Task AddAsync(Transaction transaction)
        {
            const string sql = @"
                INSERT INTO transactions (
                    id, card_id, type, reward_id, quantity,
                    points_amount, transaction_amount, store_id, staff_id,
                    pos_transaction_id, timestamp, created_at, metadata
                ) VALUES (
                    @Id, @CardId, @Type::transaction_type, @RewardId, @Quantity,
                    @PointsAmount, @TransactionAmount, @StoreId, @StaffId,
                    @PosTransactionId, @Timestamp, @CreatedAt, @Metadata
                )";

            var connection = await _dbConnection.GetConnectionAsync();
            await connection.ExecuteAsync(sql, new
            {
                transaction.Id,
                transaction.CardId,
                Type = transaction.Type.ToString(),
                transaction.RewardId,
                transaction.Quantity,
                transaction.PointsAmount,
                transaction.TransactionAmount,
                transaction.StoreId,
                transaction.StaffId,
                transaction.PosTransactionId,
                transaction.Timestamp,
                transaction.CreatedAt,
                Metadata = transaction.Metadata
            });
        }
    }
} 