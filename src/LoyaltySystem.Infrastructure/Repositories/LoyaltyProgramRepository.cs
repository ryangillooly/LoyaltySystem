using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Infrastructure.Data;

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
        public async Task<LoyaltyProgram> GetByIdAsync(Guid id)
        {
            const string sql = @"
                SELECT 
                    p.Id, p.BrandId, p.Name, p.Type, p.StampThreshold, p.PointsConversionRate,
                    p.DailyStampLimit, p.MinimumTransactionAmount, p.IsActive, p.CreatedAt, p.UpdatedAt,
                    e.HasExpiration, e.ExpirationType, e.ExpirationValue, 
                    e.ExpiresOnSpecificDate, e.ExpirationDay, e.ExpirationMonth
                FROM LoyaltyPrograms p
                LEFT JOIN ProgramExpirationPolicies e ON p.Id = e.ProgramId
                WHERE p.Id = @Id";

            var connection = await _dbConnection.GetConnectionAsync();
            
            var programDictionary = new Dictionary<Guid, LoyaltyProgram>();
            
            var programs = await connection.QueryAsync<LoyaltyProgram, ExpirationPolicy, LoyaltyProgram>(
                sql,
                (program, expirationPolicy) =>
                {
                    if (!programDictionary.TryGetValue(program.Id, out var existingProgram))
                    {
                        existingProgram = program;
                        existingProgram.SetExpirationPolicy(expirationPolicy);
                        programDictionary.Add(existingProgram.Id, existingProgram);
                    }
                    
                    return existingProgram;
                },
                new { Id = id },
                splitOn: "HasExpiration");
                
            var result = programs.FirstOrDefault();
            
            if (result != null)
            {
                // Load rewards separately
                await LoadRewardsAsync(result);
            }
            
            return result;
        }

        /// <summary>
        /// Gets loyalty programs for a specific brand.
        /// </summary>
        public async Task<IEnumerable<LoyaltyProgram>> GetByBrandIdAsync(Guid brandId)
        {
            const string sql = @"
                SELECT 
                    p.Id, p.BrandId, p.Name, p.Type, p.StampThreshold, p.PointsConversionRate,
                    p.DailyStampLimit, p.MinimumTransactionAmount, p.IsActive, p.CreatedAt, p.UpdatedAt,
                    e.HasExpiration, e.ExpirationType, e.ExpirationValue, 
                    e.ExpiresOnSpecificDate, e.ExpirationDay, e.ExpirationMonth
                FROM LoyaltyPrograms p
                LEFT JOIN ProgramExpirationPolicies e ON p.Id = e.ProgramId
                WHERE p.BrandId = @BrandId
                ORDER BY p.Name";

            var connection = await _dbConnection.GetConnectionAsync();
            
            var programDictionary = new Dictionary<Guid, LoyaltyProgram>();
            
            var programs = await connection.QueryAsync<LoyaltyProgram, ExpirationPolicy, LoyaltyProgram>(
                sql,
                (program, expirationPolicy) =>
                {
                    if (!programDictionary.TryGetValue(program.Id, out var existingProgram))
                    {
                        existingProgram = program;
                        existingProgram.SetExpirationPolicy(expirationPolicy);
                        programDictionary.Add(existingProgram.Id, existingProgram);
                    }
                    
                    return existingProgram;
                },
                new { BrandId = brandId },
                splitOn: "HasExpiration");
                
            var result = programs.Distinct().ToList();
            
            foreach (var program in result)
            {
                await LoadRewardsAsync(program);
            }
            
            return result;
        }

        /// <summary>
        /// Gets active loyalty programs for a specific brand.
        /// </summary>
        public async Task<IEnumerable<LoyaltyProgram>> GetActiveByBrandIdAsync(Guid brandId)
        {
            const string sql = @"
                SELECT 
                    p.Id, p.BrandId, p.Name, p.Type, p.StampThreshold, p.PointsConversionRate,
                    p.DailyStampLimit, p.MinimumTransactionAmount, p.IsActive, p.CreatedAt, p.UpdatedAt,
                    e.HasExpiration, e.ExpirationType, e.ExpirationValue, 
                    e.ExpiresOnSpecificDate, e.ExpirationDay, e.ExpirationMonth
                FROM LoyaltyPrograms p
                LEFT JOIN ProgramExpirationPolicies e ON p.Id = e.ProgramId
                WHERE p.BrandId = @BrandId AND p.IsActive = 1
                ORDER BY p.Name";

            var connection = await _dbConnection.GetConnectionAsync();
            
            var programDictionary = new Dictionary<Guid, LoyaltyProgram>();
            
            var programs = await connection.QueryAsync<LoyaltyProgram, ExpirationPolicy, LoyaltyProgram>(
                sql,
                (program, expirationPolicy) =>
                {
                    if (!programDictionary.TryGetValue(program.Id, out var existingProgram))
                    {
                        existingProgram = program;
                        existingProgram.SetExpirationPolicy(expirationPolicy);
                        programDictionary.Add(existingProgram.Id, existingProgram);
                    }
                    
                    return existingProgram;
                },
                new { BrandId = brandId },
                splitOn: "HasExpiration");
                
            var result = programs.Distinct().ToList();
            
            foreach (var program in result)
            {
                await LoadRewardsAsync(program);
            }
            
            return result;
        }

        /// <summary>
        /// Gets loyalty programs by type.
        /// </summary>
        public async Task<IEnumerable<LoyaltyProgram>> GetByTypeAsync(LoyaltyProgramType type, int skip, int take)
        {
            const string sql = @"
                SELECT 
                    p.Id, p.BrandId, p.Name, p.Type, p.StampThreshold, p.PointsConversionRate,
                    p.DailyStampLimit, p.MinimumTransactionAmount, p.IsActive, p.CreatedAt, p.UpdatedAt,
                    e.HasExpiration, e.ExpirationType, e.ExpirationValue, 
                    e.ExpiresOnSpecificDate, e.ExpirationDay, e.ExpirationMonth
                FROM LoyaltyPrograms p
                LEFT JOIN ProgramExpirationPolicies e ON p.Id = e.ProgramId
                WHERE p.Type = @Type
                ORDER BY p.BrandId, p.Name
                OFFSET @Skip ROWS
                FETCH NEXT @Take ROWS ONLY";

            var connection = await _dbConnection.GetConnectionAsync();
            
            var programDictionary = new Dictionary<Guid, LoyaltyProgram>();
            
            var programs = await connection.QueryAsync<LoyaltyProgram, ExpirationPolicy, LoyaltyProgram>(
                sql,
                (program, expirationPolicy) =>
                {
                    if (!programDictionary.TryGetValue(program.Id, out var existingProgram))
                    {
                        existingProgram = program;
                        existingProgram.SetExpirationPolicy(expirationPolicy);
                        programDictionary.Add(existingProgram.Id, existingProgram);
                    }
                    
                    return existingProgram;
                },
                new { Type = (int)type, Skip = skip, Take = take },
                splitOn: "HasExpiration");
                
            var result = programs.Distinct().ToList();
            
            foreach (var program in result)
            {
                await LoadRewardsAsync(program);
            }
            
            return result;
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
        public async Task<IEnumerable<Reward>> GetRewardsForProgramAsync(Guid programId)
        {
            const string sql = @"
                SELECT 
                    r.Id, r.ProgramId, r.Title, r.Description, r.RequiredValue,
                    r.ValidFrom, r.ValidTo, r.IsActive, r.CreatedAt, r.UpdatedAt
                FROM Rewards r
                WHERE r.ProgramId = @ProgramId
                ORDER BY r.RequiredValue";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.QueryAsync<Reward>(sql, new { ProgramId = programId });
        }

        /// <summary>
        /// Gets active rewards for a specific program.
        /// </summary>
        public async Task<IEnumerable<Reward>> GetActiveRewardsForProgramAsync(Guid programId)
        {
            var now = DateTime.UtcNow;
            
            const string sql = @"
                SELECT 
                    r.Id, r.ProgramId, r.Title, r.Description, r.RequiredValue,
                    r.ValidFrom, r.ValidTo, r.IsActive, r.CreatedAt, r.UpdatedAt
                FROM Rewards r
                WHERE r.ProgramId = @ProgramId
                AND r.IsActive = 1
                AND (r.ValidFrom IS NULL OR r.ValidFrom <= @Now)
                AND (r.ValidTo IS NULL OR r.ValidTo >= @Now)
                ORDER BY r.RequiredValue";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.QueryAsync<Reward>(sql, new { ProgramId = programId, Now = now });
        }

        /// <summary>
        /// Gets a specific reward.
        /// </summary>
        public async Task<Reward> GetRewardByIdAsync(Guid rewardId)
        {
            const string sql = @"
                SELECT 
                    r.Id, r.ProgramId, r.Title, r.Description, r.RequiredValue,
                    r.ValidFrom, r.ValidTo, r.IsActive, r.CreatedAt, r.UpdatedAt
                FROM Rewards r
                WHERE r.Id = @Id";

            var connection = await _dbConnection.GetConnectionAsync();
            return await connection.QueryFirstOrDefaultAsync<Reward>(sql, new { Id = rewardId });
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
        private async Task LoadRewardsAsync(LoyaltyProgram program)
        {
            var rewards = await GetRewardsForProgramAsync(program.Id);
            foreach (var reward in rewards)
            {
                program.AddReward(reward);
            }
        }

        /// <summary>
        /// Adds a reward to the database, optionally as part of a transaction.
        /// </summary>
        private async Task AddRewardAsync(Reward reward, IDbTransaction transaction = null)
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