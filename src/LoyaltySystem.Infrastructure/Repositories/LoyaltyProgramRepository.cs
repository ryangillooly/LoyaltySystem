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

        /// <summary>
        /// Gets a loyalty program by its ID.
        /// </summary>
        public async Task<LoyaltyProgram> GetByIdAsync(LoyaltyProgramId id)
        {
            const string sql = @"
                SELECT * FROM LoyaltyPrograms WHERE Id = @Id";

            var dbConnection = await _dbConnection.GetConnectionAsync();
            var parameters = new { Id = id.Value };
            var program = await dbConnection.QuerySingleOrDefaultAsync<LoyaltyProgram>(sql, parameters);

            if (program != null)
            {
                await LoadRewardsForProgram(program);
            }

            return program;
        }

        /// <summary>
        /// Gets loyalty programs for a specific brand.
        /// </summary>
        public async Task<IEnumerable<LoyaltyProgram>> GetByBrandIdAsync(BrandId brandId)
        {
            const string sql = @"
                SELECT * FROM LoyaltyPrograms 
                WHERE BrandId = @BrandId
                ORDER BY CreatedAt DESC";

            var dbConnection = await _dbConnection.GetConnectionAsync();
            var parameters = new { BrandId = brandId.Value };
            var programs = await dbConnection.QueryAsync<LoyaltyProgram>(sql, parameters);

            foreach (var program in programs)
            {
                await LoadRewardsForProgram(program);
            }

            return programs;
        }

        /// <summary>
        /// Gets active loyalty programs for a specific brand.
        /// </summary>
        public async Task<IEnumerable<LoyaltyProgram>> GetActiveByBrandIdAsync(BrandId brandId)
        {
            const string sql = @"
                SELECT * FROM LoyaltyPrograms 
                WHERE BrandId = @BrandId AND IsActive = @IsActive
                ORDER BY CreatedAt DESC";

            var dbConnection = await _dbConnection.GetConnectionAsync();
            var parameters = new { BrandId = brandId.Value, IsActive = true };
            var programs = await dbConnection.QueryAsync<LoyaltyProgram>(sql, parameters);

            foreach (var program in programs)
            {
                await LoadRewardsForProgram(program);
            }

            return programs;
        }

        /// <summary>
        /// Gets loyalty programs by type.
        /// </summary>
        public async Task<IEnumerable<LoyaltyProgram>> GetByTypeAsync(LoyaltyProgramType type, int page, int pageSize)
        {
            const string sql = @"
                SELECT * FROM LoyaltyPrograms 
                WHERE Type = @Type
                ORDER BY CreatedAt DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            var dbConnection = await _dbConnection.GetConnectionAsync();
            var parameters = new { Type = (int)type, Offset = (page - 1) * pageSize, PageSize = pageSize };
            var programs = await dbConnection.QueryAsync<LoyaltyProgram>(sql, parameters);

            foreach (var program in programs)
            {
                await LoadRewardsForProgram(program);
            }

            return programs;
        }

        /// <summary>
        /// Adds a new loyalty program.
        /// </summary>
        public async Task AddAsync(LoyaltyProgram program)
        {
            const string insertProgramSql = @"
                INSERT INTO LoyaltyPrograms (
                    Id, BrandId, Name, Type, StampThreshold, PointsConversionRate,
                    DailyStampLimit, MinimumTransactionAmount, IsActive, CreatedAt, UpdatedAt
                ) VALUES (
                    @Id, @BrandId, @Name, @Type, @StampThreshold, @PointsConversionRate,
                    @DailyStampLimit, @MinimumTransactionAmount, @IsActive, @CreatedAt, @UpdatedAt
                )";
                
            const string insertExpirationSql = @"
                INSERT INTO ProgramExpirationPolicies (
                    ProgramId, HasExpiration, ExpirationType, ExpirationValue,
                    ExpiresOnSpecificDate, ExpirationDay, ExpirationMonth
                ) VALUES (
                    @ProgramId, @HasExpiration, @ExpirationType, @ExpirationValue,
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
                        Type = (int)program.Type,
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
                            ExpirationType = (int)program.ExpirationPolicy.ExpirationType, 
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

        /// <summary>
        /// Updates an existing loyalty program.
        /// </summary>
        public async Task UpdateAsync(LoyaltyProgram program)
        {
            const string updateProgramSql = @"
                UPDATE LoyaltyPrograms
                SET Name = @Name, 
                    StampThreshold = @StampThreshold, 
                    PointsConversionRate = @PointsConversionRate,
                    DailyStampLimit = @DailyStampLimit, 
                    MinimumTransactionAmount = @MinimumTransactionAmount, 
                    IsActive = @IsActive,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";
                
            const string updateExpirationSql = @"
                UPDATE ProgramExpirationPolicies
                SET HasExpiration = @HasExpiration,
                    ExpirationType = @ExpirationType,
                    ExpirationValue = @ExpirationValue,
                    ExpiresOnSpecificDate = @ExpiresOnSpecificDate,
                    ExpirationDay = @ExpirationDay,
                    ExpirationMonth = @ExpirationMonth
                WHERE ProgramId = @ProgramId";

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
                            ExpirationType = (int)program.ExpirationPolicy.ExpirationType, 
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

        /// <summary>
        /// Gets the rewards for a specific program.
        /// </summary>
        public async Task<IEnumerable<Reward>> GetRewardsForProgramAsync(LoyaltyProgramId programId)
        {
            const string sql = @"
                SELECT * FROM Rewards 
                WHERE ProgramId = @ProgramId
                ORDER BY RequiredValue";

            var dbConnection = await _dbConnection.GetConnectionAsync();
            var parameters = new { ProgramId = programId.Value };
            var rewards = await dbConnection.QueryAsync<Reward>(sql, parameters);

            return rewards;
        }

        /// <summary>
        /// Gets active rewards for a specific program.
        /// </summary>
        public async Task<IEnumerable<Reward>> GetActiveRewardsForProgramAsync(LoyaltyProgramId programId)
        {
            const string sql = @"
                SELECT * FROM Rewards 
                WHERE ProgramId = @ProgramId AND IsActive = @IsActive
                ORDER BY RequiredValue";

            var dbConnection = await _dbConnection.GetConnectionAsync();
            var parameters = new { ProgramId = programId.Value, IsActive = true };
            var rewards = await dbConnection.QueryAsync<Reward>(sql, parameters);

            return rewards;
        }

        /// <summary>
        /// Gets a specific reward.
        /// </summary>
        public async Task<Reward> GetRewardByIdAsync(RewardId rewardId)
        {
            const string sql = @"
                SELECT * FROM Rewards WHERE Id = @Id";

            var dbConnection = await _dbConnection.GetConnectionAsync();
            var parameters = new { Id = rewardId.Value };
            var reward = await dbConnection.QuerySingleOrDefaultAsync<Reward>(sql, parameters);

            return reward;
        }

        /// <summary>
        /// Adds a new reward.
        /// </summary>
        public async Task AddRewardAsync(Reward reward)
        {
            var connection = await _dbConnection.GetConnectionAsync();
            await AddRewardAsync(reward, null);
        }

        /// <summary>
        /// Updates an existing reward.
        /// </summary>
        public async Task UpdateRewardAsync(Reward reward)
        {
            const string sql = @"
                UPDATE Rewards
                SET Title = @Title,
                    Description = @Description,
                    RequiredValue = @RequiredValue,
                    ValidFrom = @ValidFrom,
                    ValidTo = @ValidTo,
                    IsActive = @IsActive,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

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

        /// <summary>
        /// Loads rewards for a loyalty program.
        /// </summary>
        private async Task LoadRewardsForProgram(LoyaltyProgram program)
        {
            var rewards = await GetRewardsForProgramAsync(new LoyaltyProgramId(program.Id));
            foreach (var reward in rewards)
            {
                program.AddReward(reward);
            }
        }

        /// <summary>
        /// Adds a reward to the database, optionally as part of a transaction.
        /// </summary>
        private async Task AddRewardAsync(Reward reward, IDbTransaction? transaction = null)
        {
            const string sql = @"
                INSERT INTO Rewards (
                    Id, ProgramId, Title, Description, RequiredValue,
                    ValidFrom, ValidTo, IsActive, CreatedAt, UpdatedAt
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
    }
} 