using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Infrastructure.Data;
using LoyaltySystem.Infrastructure.Data.Extensions;
using LoyaltySystem.Infrastructure.Data.TypeHandlers;

namespace LoyaltySystem.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for LoyaltyCard entities using Dapper.
    /// </summary>
    public class LoyaltyCardRepository : ILoyaltyCardRepository
    {
        private readonly IDatabaseConnection _dbConnection;

        static LoyaltyCardRepository()
        {
            // Initialize type handlers from centralized configuration
            TypeHandlerConfig.Initialize();
        }

        public LoyaltyCardRepository(IDatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }
        
        /// <summary>
        /// Gets a loyalty card by its ID.
        /// </summary>
        public async Task<LoyaltyCard?> GetByIdAsync(LoyaltyCardId id)
        {
            const string sql = @"
                SELECT 
                    c.id AS Id, c.program_id AS ProgramId, c.customer_id AS CustomerId, 
                    c.type::int AS Type, c.stamps_collected AS StampsCollected, 
                    c.points_balance AS PointsBalance, c.status::int AS Status, 
                    c.qr_code AS QrCode, c.created_at AS CreatedAt, 
                    c.expires_at AS ExpiresAt, c.updated_at AS UpdatedAt
                FROM loyalty_cards c
                WHERE c.id = @Id";

            var connection = await _dbConnection.GetConnectionAsync();
            var card = await connection.QueryFirstOrDefaultAsync<LoyaltyCard>(sql, new { Id = id });
            
            if (card != null)
            {
                // Load transactions separately
                await LoadTransactionsAsync(card);
            }
            
            return card;
        }
        
        /// <summary>
        /// Gets loyalty cards for a specific customer.
        /// </summary>
        public async Task<IEnumerable<LoyaltyCard>> GetByCustomerIdAsync(CustomerId customerId)
        {
            const string sql = @"
                SELECT 
                    c.id AS Id, c.program_id AS ProgramId, c.customer_id AS CustomerId, 
                    c.type::int AS Type, c.stamps_collected AS StampsCollected, 
                    c.points_balance AS PointsBalance, c.status::int AS Status, 
                    c.qr_code AS QrCode, c.created_at AS CreatedAt, 
                    c.expires_at AS ExpiresAt, c.updated_at AS UpdatedAt
                FROM loyalty_cards c
                WHERE c.customer_id = @CustomerId
                ORDER BY c.created_at DESC";

            var connection = await _dbConnection.GetConnectionAsync();
            var cards = await connection.QueryAsync<LoyaltyCard>(sql, new { CustomerId = customerId.Value });
            
            var cardsList = cards.ToList();
            foreach (var card in cardsList)
            {
                await LoadTransactionsAsync(card);
            }
            
            return cardsList;
        }
        
        /// <summary>
        /// Gets loyalty cards for a specific program.
        /// </summary>
        public async Task<IEnumerable<LoyaltyCard>> GetByProgramIdAsync(LoyaltyProgramId programId)
        {
            const string sql = @"
                SELECT 
                    c.id AS Id, c.program_id AS ProgramId, c.customer_id AS CustomerId, 
                    c.type::int AS Type, c.stamps_collected AS StampsCollected, 
                    c.points_balance AS PointsBalance, c.status::int AS Status, 
                    c.qr_code AS QrCode, c.created_at AS CreatedAt, 
                    c.expires_at AS ExpiresAt, c.updated_at AS UpdatedAt
                FROM loyalty_cards c
                WHERE c.program_id = @ProgramId
                ORDER BY c.created_at DESC
                LIMIT 100";

            var connection = await _dbConnection.GetConnectionAsync();
            var cards = await connection.QueryAsync<LoyaltyCard>(sql, new { ProgramId = programId.Value });
            
            var cardsList = cards.ToList();
            foreach (var card in cardsList)
            {
                await LoadTransactionsAsync(card);
            }
            
            return cardsList;
        }
        
        /// <summary>
        /// Gets a loyalty card by its QR code.
        /// </summary>
        public async Task<LoyaltyCard?> GetByQrCodeAsync(string qrCode)
        {
            const string sql = @"
                SELECT 
                    c.id AS Id, c.program_id AS ProgramId, c.customer_id AS CustomerId, 
                    c.type::int AS Type, c.stamps_collected AS StampsCollected, 
                    c.points_balance AS PointsBalance, c.status::int AS Status, 
                    c.qr_code AS QrCode, c.created_at AS CreatedAt, 
                    c.expires_at AS ExpiresAt, c.updated_at AS UpdatedAt
                FROM loyalty_cards c
                WHERE c.qr_code = @QrCode";

            var connection = await _dbConnection.GetConnectionAsync();
            var card = await connection.QueryFirstOrDefaultAsync<LoyaltyCard>(sql, new { QrCode = qrCode });
            
            if (card != null)
            {
                await LoadTransactionsAsync(card);
            }
            
            return card;
        }
        
        /// <summary>
        /// Adds a new loyalty card.
        /// </summary>
        public async Task AddAsync(LoyaltyCard card)
        {
            const string sql = @"
                INSERT INTO loyalty_cards (
                    id, program_id, customer_id, type, stamps_collected, 
                    points_balance, status, qr_code, created_at, expires_at, updated_at
                ) VALUES (
                    @Id, @ProgramId, @CustomerId, @Type::loyalty_program_type, @StampsCollected, 
                    @PointsBalance, @Status::card_status, @QrCode, @CreatedAt, @ExpiresAt, @UpdatedAt
                )";

            var connection = await _dbConnection.GetConnectionAsync();
            
            using (var transaction = await connection.BeginTransactionAsync())
            {
                try
                {
                    await connection.ExecuteAsync(sql, new
                    {
                        card.Id,
                        card.ProgramId,
                        card.CustomerId,
                        Type = card.Type.ToString(),
                        card.StampsCollected,
                        card.PointsBalance,
                        Status = card.Status.ToString(),
                        card.QrCode,
                        card.CreatedAt,
                        card.ExpiresAt,
                        card.UpdatedAt
                    }, transaction);
                    
                    // Add any initial transactions
                    foreach (var tx in card.Transactions)
                    {
                        await AddTransactionAsync(tx, transaction);
                    }
                    
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        
        /// <summary>
        /// Updates an existing loyalty card.
        /// </summary>
        public async Task UpdateAsync(LoyaltyCard card)
        {
            const string sql = @"
                UPDATE loyalty_cards
                SET stamps_collected = @StampsCollected,
                    points_balance = @PointsBalance,
                    status = @Status::card_status,
                    expires_at = @ExpiresAt,
                    updated_at = @UpdatedAt
                WHERE id = @Id";

            var connection = await _dbConnection.GetConnectionAsync();
            await connection.ExecuteAsync(sql, new
            {
                card.Id,
                card.StampsCollected,
                card.PointsBalance,
                Status = card.Status.ToString(),
                card.ExpiresAt,
                card.UpdatedAt
            });
        }
        
        /// <summary>
        /// Finds cards that are about to expire.
        /// </summary>
        public async Task<IEnumerable<LoyaltyCard>> FindCardsNearExpirationAsync(int daysUntilExpiration)
        {
            const string sql = @"
                SELECT 
                    c.id AS Id, c.program_id AS ProgramId, c.customer_id AS CustomerId, 
                    c.type::int AS Type, c.stamps_collected AS StampsCollected, 
                    c.points_balance AS PointsBalance, c.status::int AS Status, 
                    c.qr_code AS QrCode, c.created_at AS CreatedAt, 
                    c.expires_at AS ExpiresAt, c.updated_at AS UpdatedAt
                FROM loyalty_cards c
                WHERE c.status = 'Active'::card_status 
                AND c.expires_at IS NOT NULL
                AND c.expires_at BETWEEN CURRENT_DATE AND (CURRENT_DATE + INTERVAL '@DaysUntilExpiration days')
                ORDER BY c.expires_at ASC";

            var connection = await _dbConnection.GetConnectionAsync();
            var cards = await connection.QueryAsync<LoyaltyCard>(sql, new { DaysUntilExpiration = daysUntilExpiration });
            
            return cards;
        }
        
        /// <summary>
        /// Gets the count of active cards for a specific program.
        /// </summary>
        public async Task<int> GetActiveCardCountForProgramAsync(LoyaltyProgramId programId)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM loyalty_cards
                WHERE program_id = @ProgramId
                AND status = 'Active'::card_status";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.ExecuteScalarAsync<int>(sql, new { ProgramId = programId.Value });
        }
        
        /// <summary>
        /// Gets the count of cards by status.
        /// </summary>
        public async Task<int> GetCardCountByStatusAsync(CardStatus status)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM loyalty_cards
                WHERE status = @Status::card_status";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.ExecuteScalarAsync<int>(sql, new { Status = status.ToString() });
        }
        
        /// <summary>
        /// Find cards by status with pagination.
        /// </summary>
        public async Task<IEnumerable<LoyaltyCard>> FindByStatusAsync(CardStatus status, int skip, int take)
        {
            const string sql = @"
                SELECT 
                    c.id AS Id, c.program_id AS ProgramId, c.customer_id AS CustomerId, 
                    c.type::int AS Type, c.stamps_collected AS StampsCollected, 
                    c.points_balance AS PointsBalance, c.status::int AS Status, 
                    c.qr_code AS QrCode, c.created_at AS CreatedAt, 
                    c.expires_at AS ExpiresAt, c.updated_at AS UpdatedAt
                FROM loyalty_cards c
                WHERE c.status = @Status::card_status
                ORDER BY c.created_at DESC
                LIMIT @Take OFFSET @Skip";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.QueryAsync<LoyaltyCard>(sql, new { Status = status.ToString(), Skip = skip, Take = take });
        }
        
        /// <summary>
        /// Gets a card by ID and loads all its transactions.
        /// </summary>
        public async Task<LoyaltyCard?> GetByIdWithTransactionsAsync(LoyaltyCardId id)
        {
            var card = await GetByIdAsync(id);
            return card;
        }
        
        /// <summary>
        /// Loads all transactions for a card.
        /// </summary>
        private async Task LoadTransactionsAsync(LoyaltyCard card)
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
                WHERE t.card_id = @CardId
                ORDER BY t.timestamp DESC";

            var connection = await _dbConnection.GetConnectionAsync();
            var transactions = await connection.QueryAsync<Transaction>(sql, new { CardId = card.Id });
            
            // Add each transaction to the card using the AddTransaction method
            foreach (var transaction in transactions)
            {
                card.AddTransaction(transaction);
            }
        }
        
        /// <summary>
        /// Adds a transaction with an optional transaction context.
        /// </summary>
        private async Task AddTransactionAsync(Transaction transaction, IDbTransaction? dbTransaction = null)
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
            }, dbTransaction);
        }
    }
} 