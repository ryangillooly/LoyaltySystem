using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Infrastructure.Data;
using LoyaltySystem.Infrastructure.Data.TypeHandlers;

namespace LoyaltySystem.Infrastructure.Repositories
{
    public class LoyaltyRewardsRepository : ILoyaltyRewardsRepository
    {
        private readonly IDatabaseConnection _dbConnection;

        static LoyaltyRewardsRepository()
        {
            // Initialize type handlers from centralized configuration
            TypeHandlerConfig.Initialize();
        }
        
        public LoyaltyRewardsRepository(IDatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        public async Task<Reward> GetByIdAsync(RewardId id)
        {
            const string sql = @"
                SELECT 
                    id AS Id,
                    program_id AS ProgramId,
                    title AS Title,
                    description AS Description,
                    required_value AS RequiredValue,
                    valid_from AS ValidFrom,
                    valid_to AS ValidTo,
                    is_active AS IsActive,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt
                FROM rewards 
                WHERE id = @Id";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.QuerySingleOrDefaultAsync<Reward>(sql, new { Id = id.Value });
        }

        public async Task<IEnumerable<Reward>> GetByProgramIdAsync(LoyaltyProgramId programId)
        {
            const string sql = @"
                SELECT 
                    id AS Id,
                    program_id AS ProgramId,
                    title AS Title,
                    description AS Description,
                    required_value AS RequiredValue,
                    valid_from AS ValidFrom,
                    valid_to AS ValidTo,
                    is_active AS IsActive,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt
                FROM rewards 
                WHERE program_id = @ProgramId
                ORDER BY required_value";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.QueryAsync<Reward>(sql, new { ProgramId = programId.Value });
        }

        public async Task<IEnumerable<Reward>> GetActiveByProgramIdAsync(LoyaltyProgramId programId)
        {
            const string sql = @"
                SELECT 
                    id AS Id,
                    program_id AS ProgramId,
                    title AS Title,
                    description AS Description,
                    required_value AS RequiredValue,
                    valid_from AS ValidFrom,
                    valid_to AS ValidTo,
                    is_active AS IsActive,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt
                FROM rewards 
                WHERE program_id = @ProgramId 
                    AND is_active = true
                    AND (valid_from IS NULL OR valid_from <= @Now)
                    AND (valid_to IS NULL OR valid_to >= @Now)
                ORDER BY required_value";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.QueryAsync<Reward>(sql, new { ProgramId = programId.Value, Now = DateTime.UtcNow });
        }

        public async Task<Reward> AddAsync(Reward reward)
        {
            var connection = await _dbConnection.GetConnectionAsync();
            await connection.ExecuteAsync(@"
                INSERT INTO 
                    rewards 
                        (id, program_id, title, description, required_value,
                        valid_from, valid_to, is_active, created_at, updated_at) 
                    VALUES 
                        (@Id, @ProgramId, @Title, @Description, @RequiredValue, 
                        @ValidFrom, @ValidTo, @IsActive, @CreatedAt, @UpdatedAt)
                ", 
                reward
            );
            
            return reward;
        }

        public async Task UpdateAsync(Reward reward)
        {
            const string sql = @"
                UPDATE rewards
                SET title = @Title,
                    description = @Description,
                    required_value = @RequiredValue,
                    valid_from = @ValidFrom,
                    valid_to = @ValidTo,
                    is_active = @IsActive,
                    updated_at = @UpdatedAt
                WHERE id = @Id";

            var connection = await _dbConnection.GetConnectionAsync();
            await connection.ExecuteAsync(sql, reward);
        }

        public async Task<bool> DeleteAsync(RewardId id)
        {
            const string sql = @"
                UPDATE rewards
                SET is_active = false,
                    updated_at = @UpdatedAt
                WHERE id = @Id";

            var connection = await _dbConnection.GetConnectionAsync();
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id.Value, UpdatedAt = DateTime.UtcNow });
            return rowsAffected > 0;
        }

        public async Task<int> GetTotalRedemptionsAsync(RewardId rewardId)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM transactions
                WHERE reward_id = @RewardId
                AND type = 'RewardRedemption'";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.ExecuteScalarAsync<int>(sql, new { RewardId = rewardId.Value });
        }

        public async Task<int> GetRedemptionsInLastDaysAsync(RewardId rewardId, int days)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM transactions
                WHERE reward_id = @RewardId
                AND type = 'RewardRedemption'
                AND created_at >= @StartDate";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.ExecuteScalarAsync<int>(sql, new 
            { 
                RewardId = rewardId.Value,
                StartDate = DateTime.UtcNow.AddDays(-days)
            });
        }

        public async Task<decimal> GetTotalPointsRedeemedAsync(RewardId rewardId)
        {
            const string sql = @"
                SELECT COALESCE(SUM(ABS(points_amount)), 0)
                FROM transactions
                WHERE reward_id = @RewardId
                AND type = 'RewardRedemption'";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.ExecuteScalarAsync<decimal>(sql, new { RewardId = rewardId.Value });
        }

        public async Task<decimal> GetAveragePointsPerRedemptionAsync(RewardId rewardId)
        {
            const string sql = @"
                SELECT COALESCE(AVG(ABS(points_amount)), 0)
                FROM transactions
                WHERE reward_id = @RewardId
                AND type = 'RewardRedemption'";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.ExecuteScalarAsync<decimal>(sql, new { RewardId = rewardId.Value });
        }

        public async Task<DateTime?> GetFirstRedemptionDateAsync(RewardId rewardId)
        {
            const string sql = @"
                SELECT MIN(created_at)
                FROM transactions
                WHERE reward_id = @RewardId
                AND type = 'RewardRedemption'";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.ExecuteScalarAsync<DateTime?>(sql, new { RewardId = rewardId.Value });
        }

        public async Task<DateTime?> GetLastRedemptionDateAsync(RewardId rewardId)
        {
            const string sql = @"
                SELECT MAX(created_at)
                FROM transactions
                WHERE reward_id = @RewardId
                AND type = 'RewardRedemption'";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.ExecuteScalarAsync<DateTime?>(sql, new { RewardId = rewardId.Value });
        }

        public async Task<Dictionary<string, int>> GetRedemptionsByStoreAsync(RewardId rewardId)
        {
            const string sql = @"
                SELECT s.name AS StoreName, COUNT(*) AS Count
                FROM transactions t
                JOIN stores s ON t.store_id = s.id
                WHERE t.reward_id = @RewardId
                AND t.type = 'RewardRedemption'
                GROUP BY s.name";

            var connection = await _dbConnection.GetConnectionAsync();
            var results = await connection.QueryAsync<(string StoreName, int Count)>(sql, new { RewardId = rewardId.Value });
            return results.ToDictionary(x => x.StoreName, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetRedemptionsByMonthAsync(RewardId rewardId)
        {
            const string sql = @"
                SELECT 
                    TO_CHAR(created_at, 'YYYY-MM') AS Month,
                    COUNT(*) AS Count
                FROM transactions
                WHERE reward_id = @RewardId
                AND type = 'RewardRedemption'
                GROUP BY TO_CHAR(created_at, 'YYYY-MM')
                ORDER BY Month DESC
                LIMIT 12";

            var connection = await _dbConnection.GetConnectionAsync();
            var results = await connection.QueryAsync<(string Month, int Count)>(sql, new { RewardId = rewardId.Value });
            return results.ToDictionary(x => x.Month, x => x.Count);
        }

        public async Task<int> GetTotalRewardsCountAsync(LoyaltyProgramId programId)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM rewards
                WHERE program_id = @ProgramId";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.ExecuteScalarAsync<int>(sql, new { ProgramId = programId.Value });
        }

        public async Task<int> GetActiveRewardsCountAsync(LoyaltyProgramId programId)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM rewards
                WHERE program_id = @ProgramId
                AND is_active = true
                AND (valid_from IS NULL OR valid_from <= @Now)
                AND (valid_to IS NULL OR valid_to >= @Now)";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.ExecuteScalarAsync<int>(sql, new { ProgramId = programId.Value, Now = DateTime.UtcNow });
        }

        public async Task<int> GetTotalProgramRedemptionsAsync(LoyaltyProgramId programId)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM transactions t
                JOIN rewards r ON t.reward_id = r.id
                WHERE r.program_id = @ProgramId
                AND t.type = 'RewardRedemption'";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.ExecuteScalarAsync<int>(sql, new { ProgramId = programId.Value });
        }

        public async Task<decimal> GetTotalProgramPointsRedeemedAsync(LoyaltyProgramId programId)
        {
            const string sql = @"
                SELECT COALESCE(SUM(ABS(t.points_amount)), 0)
                FROM transactions t
                JOIN rewards r ON t.reward_id = r.id
                WHERE r.program_id = @ProgramId
                AND t.type = 'RewardRedemption'";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.ExecuteScalarAsync<decimal>(sql, new { ProgramId = programId.Value });
        }

        public async Task<List<Reward>> GetTopRewardsByRedemptionsAsync(LoyaltyProgramId programId, int limit = 5)
        {
            const string sql = @"
                SELECT 
                    r.id AS Id,
                    r.program_id AS ProgramId,
                    r.title AS Title,
                    r.description AS Description,
                    r.required_value AS RequiredValue,
                    r.valid_from AS ValidFrom,
                    r.valid_to AS ValidTo,
                    r.is_active AS IsActive,
                    r.created_at AS CreatedAt,
                    r.updated_at AS UpdatedAt
                FROM rewards r
                JOIN (
                    SELECT reward_id, COUNT(*) as redemption_count
                    FROM transactions
                    WHERE type = 'RewardRedemption'
                    GROUP BY reward_id
                ) t ON r.id = t.reward_id
                WHERE r.program_id = @ProgramId
                ORDER BY t.redemption_count DESC
                LIMIT @Limit";

            var connection = await _dbConnection.GetConnectionAsync();
            return (await connection.QueryAsync<Reward>(sql, new { ProgramId = programId.Value, Limit = limit })).ToList();
        }

        public async Task<Dictionary<string, int>> GetProgramRedemptionsByTypeAsync(LoyaltyProgramId programId)
        {
            const string sql = @"
                SELECT 
                    CASE 
                        WHEN t.points_amount IS NOT NULL THEN 'Points'
                        ELSE 'Stamps'
                    END AS RewardType,
                    COUNT(*) AS Count
                FROM transactions t
                JOIN rewards r ON t.reward_id = r.id
                WHERE r.program_id = @ProgramId
                AND t.type = 'RewardRedemption'
                GROUP BY 
                    CASE 
                        WHEN t.points_amount IS NOT NULL THEN 'Points'
                        ELSE 'Stamps'
                    END";

            var connection = await _dbConnection.GetConnectionAsync();
            var results = await connection.QueryAsync<(string RewardType, int Count)>(sql, new { ProgramId = programId.Value });
            return results.ToDictionary(x => x.RewardType, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetProgramRedemptionsByMonthAsync(LoyaltyProgramId programId)
        {
            const string sql = @"
                SELECT 
                    TO_CHAR(t.created_at, 'YYYY-MM') AS Month,
                    COUNT(*) AS Count
                FROM transactions t
                JOIN rewards r ON t.reward_id = r.id
                WHERE r.program_id = @ProgramId
                AND t.type = 'RewardRedemption'
                GROUP BY TO_CHAR(t.created_at, 'YYYY-MM')
                ORDER BY Month DESC
                LIMIT 12";

            var connection = await _dbConnection.GetConnectionAsync();
            var results = await connection.QueryAsync<(string Month, int Count)>(sql, new { ProgramId = programId.Value });
            return results.ToDictionary(x => x.Month, x => x.Count);
        }
    }
} 