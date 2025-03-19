using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.LoyaltyPrograms;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.ValueObjects;
using LoyaltySystem.Infrastructure.Data;
using LoyaltySystem.Infrastructure.Data.Extensions;

namespace LoyaltySystem.Infrastructure.Repositories;

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

    public async Task<LoyaltyProgram?> GetByIdAsync(LoyaltyProgramId id)
    {
        const string sql = @"
            SELECT 
                id,
                brand_id,
                name,
                type::text,
                stamp_threshold,
                points_conversion_rate,
                daily_stamp_limit,
                minimum_transaction_amount,
                is_active,
                created_at,
                updated_at,
                has_tiers,
                points_per_pound, 
                minimum_points_for_redemption,
                points_rounding_rule,
                enrollment_bonus_points,
                description,
                terms_and_conditions,
                start_date,
                end_date
            FROM loyalty_programs 
            WHERE id = @Id::uuid";

        var dbConnection = await _dbConnection.GetConnectionAsync();
        var parameters = new { Id = id.Value };
        
        // Use a strongly-typed DTO for database mapping
        var dto = await dbConnection.QuerySingleOrDefaultAsync<LoyaltyProgramDto>(sql, parameters);
        
        if (dto == null)
            return null;
        
        // Convert DTO to domain entity
        var program = dto.ToDomain();
        
        await LoadRewardsForProgram(program);
        return program;
    }

    public async Task<IEnumerable<LoyaltyProgram>> GetByBrandIdAsync(BrandId brandId)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                brand_id AS BrandId,
                name AS Name,
                type::text AS Type,
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
                type::text AS Type,
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

    public async Task<IEnumerable<LoyaltyProgram>> GetByTypeAsync(LoyaltyProgramType type, int skip, int take)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                brand_id AS BrandId,
                name AS Name,
                (type::text)::int AS Type,
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
            LIMIT @Take OFFSET @Skip";

        var parameters = new { Type = type.ToString(), Skip = skip, Take = take };
        var connection = await _dbConnection.GetConnectionAsync();
        var programs = await connection.QueryAsync<LoyaltyProgram>(sql, parameters);

        // Load rewards for each program
        foreach (var program in programs)
        {
            await LoadRewardsForProgram(program);
        }

        return programs;
    }

    public async Task<LoyaltyProgram> AddAsync(LoyaltyProgram program, IDbTransaction transaction = null)
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
                    loyalty_programs 
                        (id, brand_id, name, type, stamp_threshold, points_conversion_rate,
                        daily_stamp_limit, minimum_transaction_amount, is_active, description, 
                        points_rounding_rule, enrollment_bonus_points, minimum_points_for_redemption,
                        points_per_pound, start_date, end_date, has_tiers, terms_and_conditions, created_at, updated_at) 
                    VALUES 
                        (@Id, @BrandId, @Name, @Type::loyalty_program_type, @StampThreshold, @PointsConversionRate,
                        @DailyStampLimit, @MinimumTransactionAmount, @IsActive, @Description, @PointsRoundingRule,
                        @EnrollmentBonusPoints, @MinimumPointsForRedemption, @PointsPerPound, @StartDate, @EndDate,
                        @HasTiers, @TermsAndConditions, @CreatedAt, @UpdatedAt)
                ",
                new
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
                    program.Description,
                    program.PointsRoundingRule,
                    program.EnrollmentBonusPoints,
                    program.MinimumPointsForRedemption, 
                    program.PointsConfig.PointsPerPound,
                    program.StartDate,
                    program.EndDate,
                    program.HasTiers,
                    program.TermsAndConditions,
                    program.CreatedAt,
                    program.UpdatedAt
                },
                transaction
            );

            if (program.ExpirationPolicy != null)
            {
                await dbConnection.ExecuteAsync(@"
                    INSERT INTO 
                        program_expiration_policies 
                            (program_id, has_expiration, expiration_type, expiration_value,
                            expires_on_specific_date, expiration_day, expiration_month) 
                        VALUES 
                            (@ProgramId, @HasExpiration, @ExpirationType::expiration_type, @ExpirationValue,
                            @ExpiresOnSpecificDate, @ExpirationDay, @ExpirationMonth)
                    ",
                    new
                    {
                        ProgramId = program.Id,
                        program.ExpirationPolicy.HasExpiration,
                        ExpirationType = program.ExpirationPolicy.ExpirationType.ToString(),
                        program.ExpirationPolicy.ExpirationValue,
                        program.ExpirationPolicy.ExpiresOnSpecificDate,
                        program.ExpirationPolicy.ExpirationDay,
                        program.ExpirationPolicy.ExpirationMonth
                    },
                    transaction
                );
            }

            // Add any rewards
            foreach (var reward in program.Rewards)
            {
                await AddRewardAsync(reward, transaction);
            }

            if (ownsTransaction)
                transaction.Commit();

            return program;
        }
        catch (Exception ex)
        {
            if (ownsTransaction)
                transaction.Rollback();

            throw new Exception($"Error adding program: {ex.Message}", ex);
        }
        finally
        {
            if (ownsTransaction && transaction != null)
                transaction.Dispose();
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
                id,
                program_id,
                title,
                description,
                required_value,
                valid_from,
                valid_to,
                is_active,
                created_at,
                updated_at
            FROM rewards 
            WHERE program_id = @ProgramId::uuid
            ORDER BY required_value";

        var dbConnection = await _dbConnection.GetConnectionAsync();
        var parameters = new { ProgramId = programId.Value };
        
        // Use RewardDto to properly map database values
        var dtos = await dbConnection.QueryAsync<RewardDto>(sql, parameters);
        
        // Convert DTOs to domain entities
        return dtos.Select(dto => dto.ToDomain());
    }

    public async Task<IEnumerable<Reward>> GetActiveRewardsForProgramAsync(LoyaltyProgramId programId)
    {
        const string sql = @"
            SELECT 
                id,
                program_id,
                title,
                description,
                required_value,
                valid_from,
                valid_to,
                is_active,
                created_at,
                updated_at
            FROM rewards 
            WHERE program_id = @ProgramId::uuid AND is_active = @IsActive
            ORDER BY required_value";

        var dbConnection = await _dbConnection.GetConnectionAsync();
        var parameters = new { ProgramId = programId.Value, IsActive = true };
        
        // Use RewardDto to properly map database values
        var dtos = await dbConnection.QueryAsync<RewardDto>(sql, parameters);
        
        // Convert DTOs to domain entities
        return dtos.Select(dto => dto.ToDomain());
    }

    public async Task<Reward> GetRewardByIdAsync(RewardId rewardId)
    {
        const string sql = @"
            SELECT 
                id,
                program_id,
                title,
                description,
                required_value,
                valid_from,
                valid_to,
                is_active,
                created_at,
                updated_at
            FROM rewards 
            WHERE id = @Id::uuid";

        var dbConnection = await _dbConnection.GetConnectionAsync();
        var parameters = new { Id = rewardId.Value };
        
        // Use RewardDto to properly map database values
        var dto = await dbConnection.QuerySingleOrDefaultAsync<RewardDto>(sql, parameters);
        
        return dto?.ToDomain();
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

    public async Task<IEnumerable<LoyaltyProgram>> GetAllAsync(int skip = 0, int limit = 50)
    {
        var sql = @"
            SELECT 
                p.id,
                p.brand_id,
                p.name,
                p.description,
                p.type::loyalty_program_type,
                p.stamp_threshold,
                p.points_conversion_rate,
                p.daily_stamp_limit,
                p.minimum_transaction_amount,
                p.is_active,
                p.created_at,
                p.updated_at,
                p.enrollment_bonus_points, 
                p.terms_and_conditions,
                p.start_date,
                p.end_date,
                p.has_tiers,
                p.points_per_pound,
                p.minimum_points_for_redemption,
                p.points_rounding_rule,
                pep.expiration_type,
                pep.expiration_day,
                pep.expiration_month,
                pep.has_expiration,
                pep.expiration_value,
                pep.expires_on_specific_date
            FROM loyalty_programs p
            LEFT JOIN program_expiration_policies pep ON p.id = pep.program_id
            ORDER BY p.created_at DESC
            LIMIT @Limit 
            OFFSET @Skip";

        var parameters = new { Skip = skip, Limit = limit };
        var dbConnection = await _dbConnection.GetConnectionAsync();
        var records = await dbConnection.QueryAsync(sql, parameters);

        var programs = new List<LoyaltyProgram>();
        foreach (var record in records)
        {
            // Create expiration policy if it exists
            ExpirationPolicy? expirationPolicy = null;
            if (record.has_expiration)
            {
                expirationPolicy = new ExpirationPolicy
                {
                    ExpirationType = Enum.Parse<ExpirationType>(record.expiration_type),
                    ExpirationDay = record.expiration_day,
                    ExpirationMonth = record.expiration_month,
                    ExpirationValue = record.expiration_value,
                    ExpiresOnSpecificDate = record.expires_on_specific_date
                };
            }

            // Parse enums with null checks and default values
            var roundingRule = record.points_rounding_rule != null 
                ? Enum.Parse<PointsRoundingRule>(record.points_rounding_rule.ToString())
                : PointsRoundingRule.RoundDown;

            var programType = record.type != null
                ? Enum.Parse<LoyaltyProgramType>(record.type.ToString())
                : LoyaltyProgramType.Stamp;

            var program = new LoyaltyProgram
            (
                new BrandId(record.brand_id),
                record.name,
                programType,
                record.stamp_threshold,
                record.points_conversion_rate,
                new PointsConfig(record.points_per_pound, record.minimum_points_for_redemption, roundingRule, record.enrollment_bonus_points),
                record.has_tiers,
                record.daily_stamp_limit,
                record.minimum_transaction_amount,
                expirationPolicy,
                record.description,
                record.terms_and_conditions,
                record.enrollment_bonus_points,
                new LoyaltyProgramId(record.id),
                record.start_date,
                record.end_date
            );

            // Set additional properties
            program.HasTiers = record.has_tiers;
            program.PointsRoundingRule = roundingRule;

            programs.Add(program);
        }

        return programs;
    }

    // DTO class for database mapping - can be moved to a separate file later
    private class LoyaltyProgramDto
    {
        public Guid id { get; set; }
        public Guid brand_id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public int stamp_threshold { get; set; }
        public decimal points_conversion_rate { get; set; }
        public int daily_stamp_limit { get; set; }
        public decimal minimum_transaction_amount { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public bool has_tiers { get; set; }
        public int points_per_pound { get; set; }
        public int minimum_points_for_redemption { get; set; }
        public int points_rounding_rule { get; set; }
        public int enrollment_bonus_points { get; set; }
        public string description { get; set; }
        public string terms_and_conditions { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        
        public LoyaltyProgram ToDomain()
        {
            return new LoyaltyProgram
            {
                Id = new LoyaltyProgramId(id),
                BrandId = new BrandId(brand_id),
                Name = name,
                Type = Enum.Parse<LoyaltyProgramType>(type),
                StampThreshold = stamp_threshold,
                PointsConversionRate = points_conversion_rate,
                DailyStampLimit = daily_stamp_limit,
                MinimumTransactionAmount = minimum_transaction_amount,
                IsActive = is_active,
                CreatedAt = created_at,
                UpdatedAt = updated_at,
                HasTiers = has_tiers,
                PointsPerPound = points_per_pound,
                MinimumPointsForRedemption = minimum_points_for_redemption,
                PointsRoundingRule = (PointsRoundingRule)points_rounding_rule,
                EnrollmentBonusPoints = enrollment_bonus_points,
                Description = description,
                TermsAndConditions = terms_and_conditions,
                StartDate = start_date,
                EndDate = end_date
            };
        }
    }
    
    // DTO for mapping reward data to/from the database
    private class RewardDto
    {
        public Guid id { get; set; }
        public Guid program_id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int required_value { get; set; }
        public DateTime? valid_from { get; set; }
        public DateTime? valid_to { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        
        public Reward ToDomain()
        {
            return new Reward
            {
                Id = new RewardId(id),
                ProgramId = new LoyaltyProgramId(program_id),
                Title = title,
                Description = description,
                RequiredValue = required_value,
                ValidFrom = valid_from,
                ValidTo = valid_to,
                IsActive = is_active,
                CreatedAt = created_at,
                UpdatedAt = updated_at
            };
        }
    }
}