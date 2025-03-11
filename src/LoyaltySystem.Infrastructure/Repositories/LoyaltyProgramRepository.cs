using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Infrastructure.Data;
using LoyaltySystem.Infrastructure.Data.Extensions;

namespace LoyaltySystem.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for LoyaltyProgram entities using Dapper.
    /// </summary>
    public class LoyaltyProgramRepository : ILoyaltyProgramRepository
    {
        private readonly IDatabaseConnection _dbConnection;

        public LoyaltyProgramRepository(IDatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }
        
        public async Task<LoyaltyProgram> GetByIdAsync(LoyaltyProgramId id)
        {
            const string sql = @"
                SELECT 
                    id AS Id,
                    brand_id AS BrandId,
                    name AS Name,
                    type::int AS Type,
                    stamp_threshold AS StampThreshold,
                    points_conversion_rate AS PointsConversionRate,
                    daily_stamp_limit AS DailyStampLimit,
                    minimum_transaction_amount AS MinimumTransactionAmount,
                    is_active AS IsActive,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt
                FROM loyalty_programs 
                WHERE id = @Id";

            var dbConnection = await _dbConnection.GetConnectionAsync();
            var parameters = new { Id = id.Value };
            var program = await dbConnection.QuerySingleOrDefaultAsync<LoyaltyProgram>(sql, parameters);

            if (program != null)
            {
                await LoadRewardsForProgram(program);
            }

            return program;
        }
        
        public async Task<IEnumerable<LoyaltyProgram>> GetByBrandIdAsync(BrandId brandId)
        {
            const string sql = @"
                SELECT 
                    id AS Id,
                    brand_id AS BrandId,
                    name AS Name,
                    type::int AS Type,
                    stamp_threshold AS StampThreshold,
                    points_conversion_rate AS PointsConversionRate,
                    daily_stamp_limit AS DailyStampLimit,
                    minimum_transaction_amount AS MinimumTransactionAmount,
                    is_active AS IsActive,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt
                FROM loyalty_programs 
                WHERE brand_id = @BrandId
                ORDER BY created_at DESC";

            var dbConnection = await _dbConnection.GetConnectionAsync();
            var parameters = new { BrandId = brandId.Value };
            var programs = await dbConnection.QueryAsync<LoyaltyProgram>(sql, parameters);

            foreach (var program in programs)
            {
                await LoadRewardsForProgram(program);
            }

            return programs;
        }
        
        public async Task<IEnumerable<LoyaltyProgram>> GetActiveByBrandIdAsync(BrandId brandId)
        {
            const string sql = @"
                SELECT 
                    id AS Id,
                    brand_id AS BrandId,
                    name AS Name,
                    type::int AS Type,
                    stamp_threshold AS StampThreshold,
                    points_conversion_rate AS PointsConversionRate,
                    daily_stamp_limit AS DailyStampLimit,
                    minimum_transaction_amount AS MinimumTransactionAmount,
                    is_active AS IsActive,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt
                FROM loyalty_programs 
                WHERE brand_id = @BrandId AND is_active = @IsActive
                ORDER BY created_at DESC";

            var dbConnection = await _dbConnection.GetConnectionAsync();
            var parameters = new { BrandId = brandId.Value, IsActive = true };
            var programs = await dbConnection.QueryAsync<LoyaltyProgram>(sql, parameters);

            foreach (var program in programs)
            {
                await LoadRewardsForProgram(program);
            }

            return programs;
        }

        public async Task<int> GetCountByTypeAsync(LoyaltyProgramType type)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM loyalty_programs
                WHERE type = @Type::loyalty_program_type
            ";
        
            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.ExecuteScalarAsync<int>(sql, new { Type = type.ToString() });
        }
        
        public async Task<IEnumerable<LoyaltyProgram>> GetByTypeAsync(LoyaltyProgramType type, int page, int pageSize)
        {
            const string sql = @"
                SELECT 
                    id AS Id,
                    brand_id AS BrandId,
                    name AS Name,
                    type::int AS Type,
                    stamp_threshold AS StampThreshold,
                    points_conversion_rate AS PointsConversionRate,
                    daily_stamp_limit AS DailyStampLimit,
                    minimum_transaction_amount AS MinimumTransactionAmount,
                    is_active AS IsActive,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt
                FROM loyalty_programs 
                WHERE type = @Type::loyalty_program_type
                ORDER BY created_at DESC
                LIMIT @PageSize OFFSET @Offset";

            var dbConnection = await _dbConnection.GetConnectionAsync();
            var parameters = new { 
                Type = type.ToString(), 
                Offset = (page - 1) * pageSize, 
                PageSize = pageSize 
            };
            var programs = await dbConnection.QueryAsync<LoyaltyProgram>(sql, parameters);

            foreach (var program in programs)
            {
                await LoadRewardsForProgram(program);
            }

            return programs;
        }

        public async Task AddAsync(LoyaltyProgram program)
        {
            const string insertProgramSql = @"
                INSERT INTO loyalty_programs (
                    id, brand_id, name, type, stamp_threshold, points_conversion_rate,
                    daily_stamp_limit, minimum_transaction_amount, is_active, created_at, updated_at
                ) VALUES (
                    @Id, @BrandId, @Name, @Type::loyalty_program_type, @StampThreshold, @PointsConversionRate,
                    @DailyStampLimit, @MinimumTransactionAmount, @IsActive, @CreatedAt, @UpdatedAt
                )";
                
            const string insertExpirationSql = @"
                INSERT INTO program_expiration_policies (
                    program_id, has_expiration, expiration_type, expiration_value,
                    expires_on_specific_date, expiration_day, expiration_month
                ) VALUES (
                    @ProgramId, @HasExpiration, @ExpirationType::expiration_type, @ExpirationValue,
                    @ExpiresOnSpecificDate, @ExpirationDay, @ExpirationMonth
                )";

            var connection = await _dbConnection.GetConnectionAsync();
            
            using (var transaction = await connection.BeginTransactionAsync())
            {
                try
                {
                    await connection.ExecuteAsync(insertProgramSql, new
                    {
                        program.Id,
                        program.BrandId,
                        program.Name,
                        Type = program.Type.ToString(),
                        program.StampThreshold,
                        program.PointsConversionRate,
                        program.DailyStampLimit,
                        program.MinimumTransactionAmount,
                        program.IsActive,
                        program.CreatedAt,
                        program.UpdatedAt
                    }, transaction);
                    
                    if (program.ExpirationPolicy != null)
                    {
                        await connection.ExecuteAsync(insertExpirationSql, new 
                        { 
                            ProgramId = program.Id, 
                            program.ExpirationPolicy.HasExpiration, 
                            ExpirationType = program.ExpirationPolicy.ExpirationType.ToString(), 
                            program.ExpirationPolicy.ExpirationValue,
                            program.ExpirationPolicy.ExpiresOnSpecificDate,
                            program.ExpirationPolicy.ExpirationDay,
                            program.ExpirationPolicy.ExpirationMonth
                        }, transaction);
                    }
                    
                    // Add any rewards
                    foreach (var reward in program.Rewards)
                    {
                        await AddRewardAsync(reward, transaction);
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

        public async Task UpdateAsync(LoyaltyProgram program)
        {
            const string updateProgramSql = @"
                UPDATE loyalty_programs
                SET name = @Name, 
                    stamp_threshold = @StampThreshold, 
                    points_conversion_rate = @PointsConversionRate,
                    daily_stamp_limit = @DailyStampLimit, 
                    minimum_transaction_amount = @MinimumTransactionAmount, 
                    is_active = @IsActive,
                    updated_at = @UpdatedAt
                WHERE id = @Id";
                
            const string updateExpirationSql = @"
                UPDATE program_expiration_policies
                SET has_expiration = @HasExpiration,
                    expiration_type = @ExpirationType::expiration_type,
                    expiration_value = @ExpirationValue,
                    expires_on_specific_date = @ExpiresOnSpecificDate,
                    expiration_day = @ExpirationDay,
                    expiration_month = @ExpirationMonth
                WHERE program_id = @ProgramId";

            var connection = await _dbConnection.GetConnectionAsync();
            
            using (var transaction = await connection.BeginTransactionAsync())
            {
                try
                {
                    await connection.ExecuteAsync(updateProgramSql, new
                    {
                        program.Id,
                        program.Name,
                        program.StampThreshold,
                        program.PointsConversionRate,
                        program.DailyStampLimit,
                        program.MinimumTransactionAmount,
                        program.IsActive,
                        program.UpdatedAt
                    }, transaction);
                    
                    if (program.ExpirationPolicy != null)
                    {
                        await connection.ExecuteAsync(updateExpirationSql, new 
                        { 
                            ProgramId = program.Id, 
                            program.ExpirationPolicy.HasExpiration, 
                            ExpirationType = program.ExpirationPolicy.ExpirationType.ToString(), 
                            program.ExpirationPolicy.ExpirationValue,
                            program.ExpirationPolicy.ExpiresOnSpecificDate,
                            program.ExpirationPolicy.ExpirationDay,
                            program.ExpirationPolicy.ExpirationMonth
                        }, transaction);
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

        public async Task<IEnumerable<Reward>> GetRewardsForProgramAsync(LoyaltyProgramId programId)
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

            var dbConnection = await _dbConnection.GetConnectionAsync();
            var parameters = new { ProgramId = programId.Value };
            var rewards = await dbConnection.QueryAsync<Reward>(sql, parameters);

            return rewards;
        }

        public async Task<IEnumerable<Reward>> GetActiveRewardsForProgramAsync(LoyaltyProgramId programId)
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
                WHERE program_id = @ProgramId AND is_active = @IsActive
                ORDER BY required_value";

            var dbConnection = await _dbConnection.GetConnectionAsync();
            var parameters = new { ProgramId = programId.Value, IsActive = true };
            var rewards = await dbConnection.QueryAsync<Reward>(sql, parameters);

            return rewards;
        }

        public async Task<Reward> GetRewardByIdAsync(RewardId rewardId)
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

            var dbConnection = await _dbConnection.GetConnectionAsync();
            var parameters = new { Id = rewardId.Value };
            var reward = await dbConnection.QuerySingleOrDefaultAsync<Reward>(sql, parameters);

            return reward;
        }

        public async Task AddRewardAsync(Reward reward)
        {
            var connection = await _dbConnection.GetConnectionAsync();
            await AddRewardAsync(reward, null);
        }

        public async Task UpdateRewardAsync(Reward reward)
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
            await connection.ExecuteAsync(sql, new
            {
                reward.Id,
                reward.Title,
                reward.Description,
                reward.RequiredValue,
                reward.ValidFrom,
                reward.ValidTo,
                reward.IsActive,
                reward.UpdatedAt
            });
        }

        private async Task LoadRewardsForProgram(LoyaltyProgram program)
        {
            var rewards = await GetRewardsForProgramAsync(new LoyaltyProgramId(program.Id));
            foreach (var reward in rewards)
            {
                program.AddReward(reward);
            }
        }

        private async Task AddRewardAsync(Reward reward, IDbTransaction? transaction = null)
        {
            const string sql = @"
                INSERT INTO rewards (
                    id, program_id, title, description, required_value,
                    valid_from, valid_to, is_active, created_at, updated_at
                ) VALUES (
                    @Id, @ProgramId, @Title, @Description, @RequiredValue,
                    @ValidFrom, @ValidTo, @IsActive, @CreatedAt, @UpdatedAt
                )";

            var connection = await _dbConnection.GetConnectionAsync();
            await connection.ExecuteAsync(sql, new
            {
                reward.Id,
                reward.ProgramId,
                reward.Title,
                reward.Description,
                reward.RequiredValue,
                reward.ValidFrom,
                reward.ValidTo,
                reward.IsActive,
                reward.CreatedAt,
                reward.UpdatedAt
            }, transaction);
        }

        public async Task<IEnumerable<LoyaltyProgram>> GetAllAsync(int skip = 0, int take = 100)
        {
            const string sql = @"
                SELECT 
                    id AS Id,
                    brand_id AS BrandId,
                    name AS Name,
                    type::int AS Type,
                    stamp_threshold AS StampThreshold,
                    points_conversion_rate AS PointsConversionRate,
                    daily_stamp_limit AS DailyStampLimit,
                    minimum_transaction_amount AS MinimumTransactionAmount,
                    is_active AS IsActive,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt
                FROM loyalty_programs 
                ORDER BY created_at DESC
                LIMIT @Take OFFSET @Skip";

            var dbConnection = await _dbConnection.GetConnectionAsync();
            var parameters = new { Skip = skip, Take = take };
            var programs = await dbConnection.QueryAsync<LoyaltyProgram>(sql, parameters);

            foreach (var program in programs)
            {
                await LoadRewardsForProgram(program);
            }

            return programs;
        }
    }
} 