using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using LoyaltySystem.Application.DTOs;
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
        
        public async Task<IEnumerable<LoyaltyCard>> GetAllAsync(int skip = 0, int limit = 50)
        {
            const string sql = @"
                SELECT * FROM loyalty_cards
                ORDER BY created_at ASC
                LIMIT @Limit 
                OFFSET @Skip";
            
            var parameters = new { Skip = skip, Limit = limit };
            var dbConnection = await _dbConnection.GetConnectionAsync();
            var dtos = await dbConnection.QueryAsync<LoyaltyCardDto>(sql, parameters);
            
            // Map DTOs to domain entities
            return dtos.Select(CreateCardFromDto).ToList();
        }
        
        public async Task<int> GetTotalCountAsync()
        {
            const string sql = "SELECT COUNT(*) FROM loyalty_cards";
            var dbConnection = await _dbConnection.GetConnectionAsync();
            return await dbConnection.ExecuteScalarAsync<int>(sql);
        }
        
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
        
        public async Task AddAsync(LoyaltyCard card, IDbTransaction transaction = null)
        {
            var dbConnection = await _dbConnection.GetConnectionAsync();
        
            // Track whether we created the transaction or are using an existing one
            bool ownsTransaction = transaction == null;
        
            // If no transaction was provided, create one
            transaction ??= dbConnection.BeginTransaction();
            
            try
            {
                await dbConnection.ExecuteAsync(@"
                    INSERT INTO 
                        loyalty_cards 
                            (id, program_id, customer_id, type, stamps_collected, 
                            points_balance, status, qr_code, created_at, expires_at, updated_at) 
                        VALUES 
                            (@Id, @ProgramId, @CustomerId, @Type::loyalty_program_type, @StampsCollected, 
                            @PointsBalance, @Status::card_status, @QrCode, @CreatedAt, @ExpiresAt, @UpdatedAt)
                    ", 
                    new
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
                    }, 
                    transaction
                );
                
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
        
        public async Task<int> GetCardCountByStatusAsync(CardStatus status)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM loyalty_cards
                WHERE status = @Status::card_status";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.ExecuteScalarAsync<int>(sql, new { Status = status.ToString() });
        }
        
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
        
        public async Task<LoyaltyCard?> GetByIdWithTransactionsAsync(LoyaltyCardId id)
        {
            var card = await GetByIdAsync(id);
            return card;
        }
        
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

        private static LoyaltyCard CreateCardFromDto(LoyaltyCardDto dto) =>
            new()
            {
                Id = dto.Id,
                CustomerId = dto.CustomerId,
                ProgramId = dto.ProgramId,
                Type = dto.Type,
                Status = dto.Status,
                QrCode = dto.QrCode,
                CreatedAt = dto.CreatedAt,
                ExpiresAt = dto.ExpiresAt,
                UpdatedAt = dto.UpdatedAt,
                StampsCollected = dto.StampCount,
                PointsBalance = dto.PointsBalance,
                Transactions = dto.Transactions?.Select(t => 
                    new Transaction
                    {
                        Id = new TransactionId(t.Id),
                        CardId = new LoyaltyCardId(t.CardId),
                        Type = t.Type,
                        RewardId = t.RewardId != null ? new RewardId(t.RewardId.Value) : null,
                        Quantity = t.Quantity,
                        PointsAmount = t.PointsAmount,
                        TransactionAmount = t.TransactionAmount,
                        StoreId = t.StoreId != null ? new StoreId(t.StoreId) : null,
                        StaffId = t.StaffId != null ? new StaffId(t.StaffId.Value) : null,
                        PosTransactionId = t.PosTransactionId,
                        Timestamp = t.Timestamp,
                        CreatedAt = t.CreatedAt,
                        Metadata = t.Metadata
                    }
                ).ToList() 
                ?? new List<Transaction>()
            };
    }
} 

