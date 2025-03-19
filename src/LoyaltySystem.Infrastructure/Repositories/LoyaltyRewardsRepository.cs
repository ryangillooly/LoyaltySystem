using Dapper;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Infrastructure.Data;
using Serilog;

namespace LoyaltySystem.Infrastructure.Repositories;

public class LoyaltyRewardsRepository : ILoyaltyRewardsRepository
{
    private readonly IDatabaseConnection _databaseConnection;
    private readonly ILogger _logger;

    public LoyaltyRewardsRepository(IDatabaseConnection databaseConnection, ILogger logger)
    {
        _databaseConnection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Reward?> GetByIdAsync(RewardId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        try
        {
            using var connection = await _databaseConnection.GetConnectionAsync();
            
            var sql = @"
                SELECT 
                    id, 
                    program_id AS ProgramId, 
                    title AS Title, 
                    description AS Description, 
                    required_value AS RequiredValue, 
                    valid_from AS ValidFrom, 
                    valid_to AS ValidTo, 
                    is_active AS IsActive, 
                    created_at AS CreatedAt, 
                    updated_at AS UpdatedAt
                FROM 
                    rewards
                WHERE 
                    id = @Id::uuid";
            
            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Id = id.Value });
            
            if (result == null)
                return null;
                
            try
            {
                // Create and return the Reward entity with proper null handling
                return new Reward(
                    new LoyaltyProgramId(result.programid),
                    result.title,
                    result.description,
                    result.requiredvalue,
                    result.validfrom,
                    result.validto
                )
                {
                    Id = id,
                    IsActive = result.isactive,
                    CreatedAt = result.createdat,
                    UpdatedAt = result.updatedat
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error mapping reward data from database for ID: {RewardId}, Data: {@Result}", id, result);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving reward with ID: {RewardId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Reward>> GetByProgramIdAsync(LoyaltyProgramId programId)
    {
        ArgumentNullException.ThrowIfNull(programId);

        try
        {
            using var connection = await _databaseConnection.GetConnectionAsync();
            
            // 1. Define SQL query with explicit type casting for UUIDs
            var sql = @"
                SELECT 
                    id::uuid AS Id, 
                    program_id::uuid AS ProgramId, 
                    title AS Title, 
                    description AS Description, 
                    required_value AS RequiredValue, 
                    valid_from AS ValidFrom, 
                    valid_to AS ValidTo, 
                    is_active AS IsActive, 
                    created_at AS CreatedAt, 
                    updated_at AS UpdatedAt
                FROM 
                    rewards
                WHERE 
                    program_id = @ProgramId::uuid
                ORDER BY 
                    required_value";
            
            // 2. Execute query
            var results = await connection.QueryAsync(sql, new { ProgramId = programId.Value });
            
            // 3. Create list to store results
            var rewards = new List<Reward>();
            
            // 4. Check if we have any results
            if (!results.Any())
            {
                _logger.Information("No rewards found for program ID: {ProgramId}", programId);
                return rewards;
            }
            
            // 5. Log first result for debugging
            _logger.Information("First result from query: {@FirstResult}", results.First());

            // 6. Process each result individually
            foreach (var result in results)
            {
                try
                {
                    // Special handling for numeric value
                    int requiredValue = 0;
                    if (result.requiredvalue != null && 
                        !int.TryParse(result.requiredvalue.ToString(), out requiredValue))
                    {
                        _logger.Warning("Failed to parse RequiredValue: {Value}", result.requiredvalue);
                    }
                    
                    // Create reward entity
                    var reward = new Reward
                    (
                        new LoyaltyProgramId(result.programid),
                        result.title,
                        result.description,
                        requiredValue,
                        result.validfrom,
                        result.validto
                    )
                    {
                        Id = new RewardId(result.id),
                        IsActive = result.isactive,
                        CreatedAt = result.createdat,
                        UpdatedAt = result.updatedat
                    };
                    
                    // Add to results
                    rewards.Add(reward);
                    
                    // Log successful mapping
                    _logger.Debug("Successfully mapped reward: {RewardId} - {Title}", reward.Id, reward.Title);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error mapping reward data: {Result}", result);
                    // Continue to next result instead of failing the entire operation
                }
            }
            
            _logger.Information("Retrieved {Count} rewards for program ID: {ProgramId}", rewards.Count, programId);
            return rewards;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving rewards for program ID: {ProgramId}", programId);
            throw;
        }
    }

    public async Task<IEnumerable<Reward>> GetActiveByProgramIdAsync(LoyaltyProgramId programId)
    {
        ArgumentNullException.ThrowIfNull(programId);

        try
        {
            using var connection = await _databaseConnection.GetConnectionAsync();
            
            // 1. Define SQL query with explicit type casting for UUIDs
            var sql = @"
                SELECT 
                    id::text AS Id, 
                    program_id::text AS ProgramId, 
                    title AS Title, 
                    description AS Description, 
                    required_value AS RequiredValue, 
                    valid_from AS ValidFrom, 
                    valid_to AS ValidTo, 
                    is_active AS IsActive, 
                    created_at AS CreatedAt, 
                    updated_at AS UpdatedAt
                FROM 
                    rewards
                WHERE 
                    program_id = @ProgramId::uuid AND 
                    is_active = true AND
                    (valid_to IS NULL OR valid_to > @Now)
                ORDER BY 
                    required_value";
            
            // 2. Execute query
            var results = await connection.QueryAsync(
                sql, 
                new { 
                    ProgramId = programId.Value,
                    Now = DateTime.UtcNow
                }
            );
            
            // 3. Create list to store results
            var rewards = new List<Reward>();
            
            // 4. Check if we have any results
            if (!results.Any())
            {
                _logger.Information("No active rewards found for program ID: {ProgramId}", programId);
                return rewards;
            }
            
            // 5. Log first result for debugging
            _logger.Information("First active result from query: {@FirstResult}", results.First());

            // 6. Process each result individually
            foreach (var result in results)
            {
                try
                {
                    // Extract and convert individual properties with null checks
                    var id = result.Id?.ToString();
                    var rewardProgramId = result.ProgramId?.ToString() ?? programId.Value.ToString();
                    var title = result.Title?.ToString() ?? "";
                    var description = result.Description?.ToString() ?? "";
                    
                    // Special handling for numeric value
                    int requiredValue = 0;
                    if (result.RequiredValue != null)
                    {
                        if (!int.TryParse(result.RequiredValue.ToString(), out requiredValue))
                        {
                            _logger.Warning("Failed to parse RequiredValue: {Value}", result.RequiredValue);
                        }
                    }

                    // Create reward entity
                    var reward = new Reward(
                        EntityId.Parse<LoyaltyProgramId>(rewardProgramId),
                        title,
                        description,
                        requiredValue,
                        result.ValidFrom,
                        result.ValidTo
                    )
                    {
                        Id = !string.IsNullOrEmpty(id) ? EntityId.Parse<RewardId>(id) : new RewardId(),
                        IsActive = result.IsActive ?? true,
                        CreatedAt = result.CreatedAt ?? DateTime.UtcNow,
                        UpdatedAt = result.UpdatedAt ?? DateTime.UtcNow
                    };
                    
                    // Add to results
                    rewards.Add(reward);
                    
                    // Log successful mapping
                    _logger.Debug("Successfully mapped active reward: {RewardId} - {Title}", reward.Id, reward.Title);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error mapping active reward data: {Result}", result);
                    // Continue to next result instead of failing the entire operation
                }
            }
            
            _logger.Information("Retrieved {Count} active rewards for program ID: {ProgramId}", rewards.Count, programId);
            return rewards;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving active rewards for program ID: {ProgramId}", programId);
            throw;
        }
    }

    public async Task<Reward> AddAsync(Reward reward)
    {
        ArgumentNullException.ThrowIfNull(reward);

        try
        {
            using var connection = await _databaseConnection.GetConnectionAsync();
            
            var sql = @"
                INSERT INTO rewards 
                (id, program_id, title, description, required_value, valid_from, valid_to, is_active, created_at, updated_at)
                VALUES 
                (@Id::uuid, @ProgramId::uuid, @Title, @Description, @RequiredValue, @ValidFrom, @ValidTo, @IsActive, @CreatedAt, @UpdatedAt)
                RETURNING id::text";

            var dto = new RewardDto
            {
                Id = reward.Id.Value,
                ProgramId = reward.ProgramId.Value,
                Title = reward.Title,
                Description = reward.Description,
                RequiredValue = reward.RequiredValue,
                ValidFrom = reward.ValidFrom,
                ValidTo = reward.ValidTo,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var id = await connection.ExecuteScalarAsync<string>(sql, dto);
            
            // Create a new reward with the returned ID
            var newReward = new Reward
            {
                Id = new RewardId(Guid.Parse(id)),
                ProgramId = reward.ProgramId,
                Title = reward.Title,
                Description = reward.Description,
                RequiredValue = reward.RequiredValue,
                ValidFrom = reward.ValidFrom,
                ValidTo = reward.ValidTo,
                IsActive = true,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
            
            return newReward;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error adding reward: {RewardTitle} for program: {ProgramId}", 
                reward.Title, reward.ProgramId);
            throw;
        }
    }

    public async Task UpdateAsync(Reward reward)
    {
        ArgumentNullException.ThrowIfNull(reward);
        if (reward.Id is null) throw new ArgumentException("Reward must have an ID to be updated", nameof(reward));

        try
        {
            using var connection = await _databaseConnection.GetConnectionAsync();
            const string sql = """
               UPDATE 
                   rewards 
               SET 
                   title = @Title,
                   description = @Description,
                   required_value = @RequiredValue,
                   valid_from = @ValidFrom,
                   valid_to = @ValidTo,
                   is_active = @IsActive,
                   updated_at = @UpdatedAt
               WHERE 
                   id = @Id::uuid
           """;

            var parameters = new
            {
                reward.Id,
                reward.Title,
                reward.Description,
                reward.RequiredValue,
                reward.ValidFrom,
                reward.ValidTo,
                IsActive = true,
                UpdatedAt = DateTime.UtcNow
            };

            var rowsAffected = await connection.ExecuteAsync(sql, parameters);
            
            if (rowsAffected == 0)
                _logger.Warning("No reward was updated with ID: {RewardId}", reward.Id);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error updating reward with ID: {RewardId}", reward.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(RewardId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        try
        {
            using var connection = await _databaseConnection.GetConnectionAsync();
            
            // First check if there are any redemptions for this reward
            var checkRedemptionsSql = @"
                SELECT COUNT(*) 
                FROM reward_redemptions 
                WHERE reward_id = @Id::uuid";
            
            var redemptionsCount = await connection.ExecuteScalarAsync<int>(checkRedemptionsSql, new { Id = id.Value });
            
            if (redemptionsCount > 0)
            {
                _logger.Warning("Cannot delete reward {RewardId} because it has {RedemptionsCount} redemptions", 
                    id, redemptionsCount);
                return false;
            }
            
            // If no redemptions, proceed with deletion
            var deleteSql = "DELETE FROM rewards WHERE id = @Id::uuid";
            var rowsAffected = await connection.ExecuteAsync(deleteSql, new { Id = id.Value });
            
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error deleting reward with ID: {RewardId}", id);
            throw;
        }
    }

    public async Task<int> GetTotalRedemptionsAsync(RewardId rewardId)
    {
        ArgumentNullException.ThrowIfNull(rewardId);

        try
        {
            using var connection = await _databaseConnection.GetConnectionAsync();
            
            var sql = @"
                SELECT COUNT(*) 
                FROM reward_redemptions 
                WHERE reward_id = @RewardId::uuid";
            
            return await connection.ExecuteScalarAsync<int>(sql, new { RewardId = rewardId.Value });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving total redemptions for reward ID: {RewardId}", rewardId);
            throw;
        }
    }

    public async Task<int> GetRedemptionsInLastDaysAsync(RewardId rewardId, int days)
    {
        ArgumentNullException.ThrowIfNull(rewardId);
        if (days <= 0) throw new ArgumentException("Days must be greater than zero", nameof(days));

        try
        {
            using var connection = await _databaseConnection.GetConnectionAsync();
            
            var sql = @"
                SELECT COUNT(*) 
                FROM reward_redemptions 
                WHERE 
                    reward_id = @RewardId::uuid AND 
                    redeemed_at >= @CutoffDate";
            
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            
            return await connection.ExecuteScalarAsync<int>(
                sql, 
                new { 
                    RewardId = rewardId.Value,
                    CutoffDate = cutoffDate
                }
            );
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving redemptions in last {Days} days for reward ID: {RewardId}", 
                days, rewardId);
            throw;
        }
    }

    public async Task<decimal> GetTotalPointsRedeemedAsync(RewardId rewardId)
    {
        ArgumentNullException.ThrowIfNull(rewardId);

        try
        {
            using var connection = await _databaseConnection.GetConnectionAsync();
            
            var sql = @"
                SELECT COALESCE(SUM(points_redeemed), 0) 
                FROM reward_redemptions 
                WHERE reward_id = @RewardId::uuid";
            
            return await connection.ExecuteScalarAsync<decimal>(sql, new { RewardId = rewardId.Value });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving total points redeemed for reward ID: {RewardId}", rewardId);
            throw;
        }
    }

    public async Task<decimal> GetAveragePointsPerRedemptionAsync(RewardId rewardId)
    {
        ArgumentNullException.ThrowIfNull(rewardId);

        try
        {
            using var connection = await _databaseConnection.GetConnectionAsync();
            
            var sql = @"
                SELECT COALESCE(AVG(points_redeemed), 0) 
                FROM reward_redemptions 
                WHERE reward_id = @RewardId::uuid";
            
            return await connection.ExecuteScalarAsync<decimal>(sql, new { RewardId = rewardId.Value });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving average points per redemption for reward ID: {RewardId}", rewardId);
            throw;
        }
    }

    public async Task<DateTime?> GetFirstRedemptionDateAsync(RewardId rewardId)
    {
        ArgumentNullException.ThrowIfNull(rewardId);

        try
        {
            using var connection = await _databaseConnection.GetConnectionAsync();
            
            var sql = @"
                SELECT MIN(redeemed_at) 
                FROM reward_redemptions 
                WHERE reward_id = @RewardId::uuid";
            
            return await connection.ExecuteScalarAsync<DateTime?>(sql, new { RewardId = rewardId.Value });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving first redemption date for reward ID: {RewardId}", rewardId);
            throw;
        }
    }

    public async Task<DateTime?> GetLastRedemptionDateAsync(RewardId rewardId)
    {
        ArgumentNullException.ThrowIfNull(rewardId);

        try
        {
            using var connection = await _databaseConnection.GetConnectionAsync();
            
            var sql = @"
                SELECT MAX(redeemed_at) 
                FROM reward_redemptions 
                WHERE reward_id = @RewardId::uuid";
            
            return await connection.ExecuteScalarAsync<DateTime?>(sql, new { RewardId = rewardId.Value });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving last redemption date for reward ID: {RewardId}", rewardId);
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetRedemptionsByStoreAsync(RewardId rewardId)
    {
        ArgumentNullException.ThrowIfNull(rewardId);

        try
        {
            using var connection = await _databaseConnection.GetConnectionAsync();
            
            var sql = @"
                SELECT 
                    s.name AS StoreName, 
                    COUNT(rr.id) AS RedemptionCount
                FROM 
                    reward_redemptions rr JOIN 
                    stores s ON rr.store_id = s.id
                WHERE 
                    rr.reward_id = @RewardId::uuid
                GROUP BY 
                    s.name
                ORDER BY 
                    COUNT(rr.id) DESC";
            
            var results = await connection.QueryAsync<StoreRedemptionResult>(sql, new { RewardId = rewardId.Value });
            
            return results.ToDictionary(r => r.StoreName, r => r.RedemptionCount);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving redemptions by store for reward ID: {RewardId}", rewardId);
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetRedemptionsByMonthAsync(RewardId rewardId)
    {
        ArgumentNullException.ThrowIfNull(rewardId);

        try
        {
            using var connection = await _databaseConnection.GetConnectionAsync();
            
            var sql = @"
                SELECT 
                    TO_CHAR(redeemed_at, 'YYYY-MM') AS Month, 
                    COUNT(*) AS RedemptionCount
                FROM 
                    reward_redemptions
                WHERE 
                    reward_id = @RewardId::uuid
                GROUP BY 
                    TO_CHAR(redeemed_at, 'YYYY-MM')
                ORDER BY 
                    Month";
            
            var results = await connection.QueryAsync<MonthRedemptionResult>(sql, new { RewardId = rewardId.Value });
            
            return results.ToDictionary(r => r.Month, r => r.RedemptionCount);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving redemptions by month for reward ID: {RewardId}", rewardId);
            throw;
        }
    }

    public async Task<int> GetTotalRewardsCountAsync(LoyaltyProgramId programId)
    {
        ArgumentNullException.ThrowIfNull(programId);

        try
        {
            using var connection = await _databaseConnection.GetConnectionAsync();
            
            var sql = @"
                SELECT COUNT(*) 
                FROM rewards 
                WHERE program_id = @ProgramId::uuid";
            
            return await connection.ExecuteScalarAsync<int>(sql, new { ProgramId = programId.Value });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving total rewards count for program ID: {ProgramId}", programId);
            throw;
        }
    }

    public async Task<int> GetActiveRewardsCountAsync(LoyaltyProgramId programId)
    {
        ArgumentNullException.ThrowIfNull(programId);

        try
        {
            using var connection = await _databaseConnection.GetConnectionAsync();
            
            var sql = @"
                SELECT COUNT(*) 
                FROM rewards 
                WHERE 
                    program_id = @ProgramId::uuid AND 
                    is_active = true AND
                    (valid_to IS NULL OR valid_to > @Now)";
            
            return await connection.ExecuteScalarAsync<int>(
                sql, 
                new { 
                    ProgramId = programId.Value,
                    Now = DateTime.UtcNow
                }
            );
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving active rewards count for program ID: {ProgramId}", programId);
            throw;
        }
    }

    public async Task<int> GetTotalProgramRedemptionsAsync(LoyaltyProgramId programId)
    {
        ArgumentNullException.ThrowIfNull(programId);

        try
        {
            using var connection = await _databaseConnection.GetConnectionAsync();
            
            var sql = @"
                SELECT COUNT(rr.id) 
                FROM reward_redemptions rr
                JOIN rewards r ON rr.reward_id = r.id
                WHERE r.program_id = @ProgramId::uuid";
            
            return await connection.ExecuteScalarAsync<int>(sql, new { ProgramId = programId.Value });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving total program redemptions for program ID: {ProgramId}", programId);
            throw;
        }
    }

    public async Task<decimal> GetTotalProgramPointsRedeemedAsync(LoyaltyProgramId programId)
    {
        ArgumentNullException.ThrowIfNull(programId);

        try
        {
            using var connection = await _databaseConnection.GetConnectionAsync();
            
            var sql = @"
                SELECT COALESCE(SUM(rr.points_redeemed), 0) 
                FROM reward_redemptions rr
                JOIN rewards r ON rr.reward_id = r.id
                WHERE r.program_id = @ProgramId::uuid";
            
            return await connection.ExecuteScalarAsync<decimal>(sql, new { ProgramId = programId.Value });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving total program points redeemed for program ID: {ProgramId}", programId);
            throw;
        }
    }

    public async Task<List<Reward>> GetTopRewardsByRedemptionsAsync(LoyaltyProgramId programId, int limit = 5)
    {
        ArgumentNullException.ThrowIfNull(programId);
        if (limit <= 0) throw new ArgumentException("Limit must be greater than zero", nameof(limit));

        try
        {
            using var connection = await _databaseConnection.GetConnectionAsync();
            
            var sql = @"
                SELECT 
                    r.id, 
                    r.program_id AS ProgramId, 
                    r.title AS Title, 
                    r.description AS Description, 
                    r.required_value AS RequiredValue, 
                    r.valid_from AS ValidFrom, 
                    r.valid_to AS ValidTo, 
                    r.is_active AS IsActive, 
                    r.created_at AS CreatedAt, 
                    r.updated_at AS UpdatedAt,
                    COUNT(rr.id) AS RedemptionCount
                FROM 
                    rewards r
                LEFT JOIN 
                    reward_redemptions rr ON r.id = rr.reward_id
                WHERE 
                    r.program_id = @ProgramId::uuid
                GROUP BY 
                    r.id
                ORDER BY 
                    COUNT(rr.id) DESC
                LIMIT @Limit";
            
            var results = await connection.QueryAsync<dynamic>(
                sql, 
                new { 
                    ProgramId = programId.Value,
                    Limit = limit
                }
            );
            
            return results.Select(r => new Reward(
                EntityId.Parse<LoyaltyProgramId>(r.ProgramId.ToString()),
                r.Title,
                r.Description,
                r.RequiredValue,
                r.ValidFrom,
                r.ValidTo
            )).ToList();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving top rewards by redemptions for program ID: {ProgramId}", programId);
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetProgramRedemptionsByTypeAsync(LoyaltyProgramId programId)
    {
        ArgumentNullException.ThrowIfNull(programId);

        try
        {
            using var connection = await _databaseConnection.GetConnectionAsync();
            
            var sql = @"
                SELECT 
                    r.title AS RewardTitle, 
                    COUNT(rr.id) AS RedemptionCount
                FROM 
                    reward_redemptions rr
                JOIN 
                    rewards r ON rr.reward_id = r.id
                WHERE 
                    r.program_id = @ProgramId::uuid
                GROUP BY 
                    r.title
                ORDER BY 
                    COUNT(rr.id) DESC";
            
            var results = await connection.QueryAsync<RewardTypeRedemptionResult>(sql, new { ProgramId = programId.Value });
            
            return results.ToDictionary(r => r.RewardTitle, r => r.RedemptionCount);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving program redemptions by type for program ID: {ProgramId}", programId);
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetProgramRedemptionsByMonthAsync(LoyaltyProgramId programId)
    {
        ArgumentNullException.ThrowIfNull(programId);

        try
        {
            using var connection = await _databaseConnection.GetConnectionAsync();
            
            var sql = @"
                SELECT 
                    TO_CHAR(rr.redeemed_at, 'YYYY-MM') AS Month, 
                    COUNT(rr.id) AS RedemptionCount
                FROM 
                    reward_redemptions rr
                JOIN 
                    rewards r ON rr.reward_id = r.id
                WHERE 
                    r.program_id = @ProgramId::uuid
                GROUP BY 
                    TO_CHAR(rr.redeemed_at, 'YYYY-MM')
                ORDER BY 
                    Month";
            
            var results = await connection.QueryAsync<MonthRedemptionResult>(sql, new { ProgramId = programId.Value });
            
            return results.ToDictionary(r => r.Month, r => r.RedemptionCount);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving program redemptions by month for program ID: {ProgramId}", programId);
            throw;
        }
    }

    // Helper classes for query results
    private class StoreRedemptionResult
    {
        public string StoreName { get; set; }
        public int RedemptionCount { get; set; }
    }

    private class MonthRedemptionResult
    {
        public string Month { get; set; }
        public int RedemptionCount { get; set; }
    }

    private class RewardTypeRedemptionResult
    {
        public string RewardTitle { get; set; }
        public int RedemptionCount { get; set; }
    }
    
    // DTO for mapping reward data to/from the database
    private class RewardDto
    {
        public Guid Id { get; set; }
        public Guid ProgramId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int RequiredValue { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public static RewardDto FromDomain(Reward reward)
        {
            return new RewardDto
            {
                Id = reward.Id.Value,
                ProgramId = reward.ProgramId.Value,
                Title = reward.Title,
                Description = reward.Description,
                RequiredValue = reward.RequiredValue,
                ValidFrom = reward.ValidFrom,
                ValidTo = reward.ValidTo,
                IsActive = reward.IsActive,
                CreatedAt = reward.CreatedAt,
                UpdatedAt = reward.UpdatedAt
            };
        }
        
        public Reward ToDomain()
        {
            return new Reward
            {
                Id = new RewardId(Id),
                ProgramId = new LoyaltyProgramId(ProgramId),
                Title = Title,
                Description = Description,
                RequiredValue = RequiredValue,
                ValidFrom = ValidFrom,
                ValidTo = ValidTo,
                IsActive = IsActive,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt
            };
        }
    }
}
