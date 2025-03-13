using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.LoyaltyPrograms;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltySystem.Application.Services;
public class LoyaltyProgramService
{
    private readonly ILoyaltyProgramRepository _programRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LoyaltyProgramService(
        ILoyaltyProgramRepository programRepository,
        IBrandRepository brandRepository,
        IUnitOfWork unitOfWork)
    {
        _programRepository = programRepository ?? throw new ArgumentNullException(nameof(programRepository));
        _brandRepository = brandRepository ?? throw new ArgumentNullException(nameof(brandRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<OperationResult<LoyaltyProgramDto>> GetProgramByIdAsync(string id)
    {
        try
        {
            var programId = EntityId.Parse<LoyaltyProgramId>(id);
            var program = await _programRepository.GetByIdAsync(programId);

            return program is null 
                ? OperationResult<LoyaltyProgramDto>.FailureResult($"Loyalty program with ID {id} not found") 
                : OperationResult<LoyaltyProgramDto>.SuccessResult(MapToDto(program));
        }
        catch (Exception ex)
        {
            return OperationResult<LoyaltyProgramDto>.FailureResult($"Failed to get loyalty program: {ex.Message}");
        }
    }

    public async Task<OperationResult<PagedResult<LoyaltyProgramDto>>> GetAllProgramsAsync(string brandId, int skip, int limit)
    {
        try
        {
            List<LoyaltyProgram> programs;
            
            if (string.IsNullOrEmpty(brandId))
            {
                // If no brand ID is specified, get all programs with pagination to avoid loading everything at once
                programs = (List<LoyaltyProgram>) await _programRepository.GetAllAsync(skip, limit);
            }
            else
            {
                // For brand-specific filtering, we still need to get all for that brand
                // as the repository might not have a paginated version for brand filtering
                var brandIdObj = EntityId.Parse<BrandId>(brandId);
                programs = (List<LoyaltyProgram>) await _programRepository.GetByBrandIdAsync(brandIdObj);
                
                // Apply pagination manually if needed
                if (programs.Count > limit)
                    programs = programs.Skip(skip).Take(limit).ToList();
            }
            
            var programDtos = programs.Select(MapToDto).ToList();

            // For accurate total count, we would need a separate count query
            // For now, use approximate counting based on what we have
            int totalCount = string.IsNullOrEmpty(brandId) 
                ? programs.Count * limit // Rough estimate for all programs TODO: Not sure this is right
                : programs.Count; // For brand filtering, we have the actual count
            
            var result = new PagedResult<LoyaltyProgramDto>(programDtos, totalCount, skip, limit);

            return OperationResult<PagedResult<LoyaltyProgramDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return OperationResult<PagedResult<LoyaltyProgramDto>>.FailureResult($"Failed to get loyalty programs: {ex.Message}");
        }
    }

    public async Task<OperationResult<List<LoyaltyProgramDto>>> GetProgramsByBrandIdAsync(string brandId)
    {
        try
        {
            var brandIdObj = new BrandId(Guid.Parse(brandId));
            var programs = await _programRepository.GetByBrandIdAsync(brandIdObj);
            var programDtos = programs.Select(MapToDto).ToList();
            
            return OperationResult<List<LoyaltyProgramDto>>.SuccessResult(programDtos);
        }
        catch (Exception ex)
        {
            return OperationResult<List<LoyaltyProgramDto>>.FailureResult($"Failed to get loyalty programs: {ex.Message}");
        }
    }

    public async Task<OperationResult<PagedResult<LoyaltyProgramDto>>> GetProgramsByTypeAsync(string type, int skip, int limit)
    {
        try
        {
            if (!Enum.TryParse<LoyaltyProgramType>(type, true, out var programType))
            {
                return OperationResult<PagedResult<LoyaltyProgramDto>>.FailureResult($"Invalid program type: {type}");
            }
            
            var programs = await _programRepository.GetByTypeAsync(programType, skip, limit);
            var totalCount = await _programRepository.GetCountByTypeAsync(programType);
            
            var programDtos = programs.Select(MapToDto).ToList();
            var result = new PagedResult<LoyaltyProgramDto>(programDtos, totalCount, skip, limit);
            
            return OperationResult<PagedResult<LoyaltyProgramDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return OperationResult<PagedResult<LoyaltyProgramDto>>.FailureResult($"Failed to get loyalty programs by type: {ex.Message}");
        }
    }

    public async Task<OperationResult<LoyaltyProgramDto>> CreateProgramAsync(CreateLoyaltyProgramDto dto)
    {
        try
        {
            // Create expiration policy if provided
            ExpirationPolicy expirationPolicy = null;
            if (dto.ExpirationPolicy != null)
            {
                expirationPolicy = new ExpirationPolicy
                {                    
                    ExpirationDay = dto.ExpirationPolicy.ExpirationDay,
                    HasExpiration = dto.ExpirationPolicy.HasExpiration,
                    ExpirationType = dto.ExpirationPolicy.ExpirationType,
                    ExpirationValue = dto.ExpirationPolicy.ExpirationValue,
                    ExpirationMonth = dto.ExpirationPolicy.ExpirationMonth,
                    ExpiresOnSpecificDate = dto.ExpirationPolicy.ExpiresOnSpecificDate
                };
            }

            // Create points config if provided
            PointsConfig pointsConfig = null;
            if (dto.PointsConfig != null)
            {
                pointsConfig = dto.PointsConfig.ToPointsConfig();
            }

            var program = new LoyaltyProgram
            {
                Id = new LoyaltyProgramId(),
                BrandId = EntityId.Parse<BrandId>(dto.BrandId),
                Name = dto.Name,
                Description = dto.Description,
                Type = dto.Type,
                ExpirationPolicy = dto.ExpirationPolicy.ToExpirationPolicy(),
                StampThreshold = dto.StampThreshold,
                PointsConversionRate = dto.PointsConversionRate,
                PointsConfig = dto.PointsConfig.ToPointsConfig(),
                DailyStampLimit = dto.DailyStampLimit,
                MinimumTransactionAmount = dto.MinimumTransactionAmount,
                IsActive = dto.IsActive,
                HasTiers = dto.HasTiers,
                TermsAndConditions = dto.TermsAndConditions,
                EnrollmentBonusPoints = dto.EnrollmentBonusPoints,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add tiers if the program supports them
            if (dto is { HasTiers: true, Tiers: { } } && dto.Tiers.Any())
            {
                foreach (var tierDto in dto.Tiers)
                {
                    var tier = new LoyaltyTier(
                        program.Id,
                        tierDto.Name,
                        tierDto.PointThreshold,
                        tierDto.PointMultiplier,
                        tierDto.TierOrder
                    );
                    
                    if (tierDto.Benefits != null)
                    {
                        foreach (var benefit in tierDto.Benefits)
                        {
                            tier.AddBenefit(benefit);
                        }
                    }
                    
                    program.AddTier(tier);
                }
            }

            await _unitOfWork.BeginTransactionAsync();
            await _programRepository.AddAsync(program, _unitOfWork.CurrentTransaction);
            await _unitOfWork.CommitTransactionAsync();

            return OperationResult<LoyaltyProgramDto>.SuccessResult(MapToDto(program));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<LoyaltyProgramDto>.FailureResult($"Failed to create loyalty program: {ex.Message}");
        }
    }

    public async Task<OperationResult<LoyaltyProgramDto>> UpdateProgramAsync(string id, UpdateLoyaltyProgramDto dto)
    {
        try
        {
            var programId = EntityId.Parse<LoyaltyProgramId>(id);
            var program = await _programRepository.GetByIdAsync(programId);

            if (program == null)
            {
                return OperationResult<LoyaltyProgramDto>.FailureResult($"Loyalty program with ID {id} not found");
            }

            // Create expiration policy if provided
            ExpirationPolicy expirationPolicy = null;
            if (dto.ExpirationPolicy != null)
            {
                if (dto.ExpirationPolicy.ExpiresOnSpecificDate && dto.ExpirationPolicy.ExpirationDay.HasValue)
                {
                    // Create date-based expiration policy
                    expirationPolicy = new ExpirationPolicy(
                        dto.ExpirationPolicy.ExpirationDay.Value,
                        dto.ExpirationPolicy.ExpirationMonth
                    );
                }
                else if (dto.ExpirationPolicy.HasExpiration && dto.ExpirationPolicy.ExpirationValue > 0)
                {
                    // Create period-based expiration policy
                    expirationPolicy = new ExpirationPolicy(
                        dto.ExpirationPolicy.ExpirationType,
                        dto.ExpirationPolicy.ExpirationValue
                    );
                }
                else
                {
                    // Create default policy with no expiration
                    expirationPolicy = new ExpirationPolicy();
                }
            }

            // Create points config if provided
            PointsConfig pointsConfig = null;
            if (dto.PointsConfig != null)
            {
                pointsConfig = dto.PointsConfig.ToPointsConfig();
            }

            // Update the program with all properties
            program.Update(
                dto.Name,
                dto.StampThreshold,
                dto.PointsConversionRate,
                pointsConfig,
                dto.HasTiers,
                dto.DailyStampLimit,
                dto.MinimumTransactionAmount,
                expirationPolicy,
                dto.Description,
                dto.TermsAndConditions,
                dto.EnrollmentBonusPoints,
                dto.StartDate,
                dto.EndDate
            );
            
            // Handle IsActive separately if needed
            if (program.IsActive != dto.IsActive)
            {
                // Using reflection as a workaround since IsActive setter is private
                typeof(LoyaltyProgram).GetProperty("IsActive").SetValue(program, dto.IsActive);
            }

            await _unitOfWork.BeginTransactionAsync();
            await _programRepository.UpdateAsync(program);
            await _unitOfWork.CommitTransactionAsync();

            return OperationResult<LoyaltyProgramDto>.SuccessResult(MapToDto(program));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<LoyaltyProgramDto>.FailureResult($"Failed to update loyalty program: {ex.Message}");
        }
    }

    public async Task<OperationResult<RewardDto>> AddRewardToProgramAsync(string programId, CreateRewardDto dto)
    {
        try
        {
            var pId = EntityId.Parse<LoyaltyProgramId>(programId);
            var program = await _programRepository.GetByIdAsync(pId);

            if (program == null)
            {
                return OperationResult<RewardDto>.FailureResult($"Loyalty program with ID {programId} not found");
            }

            var reward = new Reward(
                program.Id,
                dto.Title,
                dto.Description,
                dto.RequiredPoints > 0 ? dto.RequiredPoints : dto.RequiredStamps,
                dto.StartDate,
                dto.EndDate
            );

            await _unitOfWork.BeginTransactionAsync();
            await _programRepository.AddRewardAsync(reward);
            await _unitOfWork.CommitTransactionAsync();

            return OperationResult<RewardDto>.SuccessResult(MapToRewardDto(reward));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<RewardDto>.FailureResult($"Failed to add reward to program: {ex.Message}");
        }
    }

    public async Task<OperationResult<RewardDto>> UpdateRewardAsync(UpdateRewardDto dto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            var rewardIdObj = new RewardId(Guid.Parse(dto.Id));
            var reward = await _programRepository.GetRewardByIdAsync(rewardIdObj);
            
            if (reward == null)
                return OperationResult<RewardDto>.FailureResult("Reward not found");
            
            reward.Update(
                dto.Title,
                dto.Description,
                dto.RequiredPoints,
                dto.StartDate,
                dto.EndDate
            );
            
            // Set IsActive state
            if (dto.IsActive)
                reward.Activate();
            else
                reward.Deactivate();
            
            await _programRepository.UpdateRewardAsync(reward);
            await _unitOfWork.CommitTransactionAsync();
            
            return OperationResult<RewardDto>.SuccessResult(MapToRewardDto(reward));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<RewardDto>.FailureResult(ex.Message);
        }
    }

    public async Task<OperationResult<List<RewardDto>>> GetRewardsByProgramIdAsync(string programId)
    {
        try
        {
            var programIdObj = new LoyaltyProgramId(Guid.Parse(programId));
            var rewards = await _programRepository.GetRewardsForProgramAsync(programIdObj);
            var rewardDtos = rewards.Select(MapToRewardDto).ToList();
            
            return OperationResult<List<RewardDto>>.SuccessResult(rewardDtos);
        }
        catch (Exception ex)
        {
            return OperationResult<List<RewardDto>>.FailureResult(ex.Message);
        }
    }

    public async Task<OperationResult<RewardDto>> GetRewardByIdAsync(string rewardId)
    {
        try
        {
            var rewardIdObj = new RewardId(Guid.Parse(rewardId));
            var reward = await _programRepository.GetRewardByIdAsync(rewardIdObj);
            
            if (reward == null)
                return OperationResult<RewardDto>.FailureResult($"Reward with ID {rewardId} not found");
            
            return OperationResult<RewardDto>.SuccessResult(MapToRewardDto(reward));
        }
        catch (Exception ex)
        {
            return OperationResult<RewardDto>.FailureResult($"Failed to get reward: {ex.Message}");
        }
    }

    public async Task<OperationResult<RewardDto>> CreateRewardAsync(string programId, CreateRewardDto dto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            var programIdObj = new LoyaltyProgramId(Guid.Parse(programId));
            var program = await _programRepository.GetByIdAsync(programIdObj);
            
            if (program == null)
                return OperationResult<RewardDto>.FailureResult("Program not found");
            
            var reward = program.CreateReward(
                dto.Title,
                dto.Description,
                dto.RequiredPoints > 0 ? dto.RequiredPoints : dto.RequiredStamps,
                dto.StartDate,
                dto.EndDate
            );
            
            await _programRepository.AddRewardAsync(reward);
            await _unitOfWork.CommitTransactionAsync();
            
            return OperationResult<RewardDto>.SuccessResult(MapToRewardDto(reward));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<RewardDto>.FailureResult(ex.Message);
        }
    }

    private LoyaltyProgramDto MapToDto(LoyaltyProgram program)
    {
        if (program == null) return null;
        
        var rewards = program.Rewards?.Select(MapToRewardDto).ToList() ?? new List<RewardDto>();
        
        var tiers = program.Tiers?.Select(t => new TierDto
        {
            Id = t.Id.ToString(),
            Name = t.Name,
            PointThreshold = t.PointThreshold,
            PointMultiplier = t.PointMultiplier,
            TierOrder = t.TierOrder,
            Benefits = t.Benefits?.ToList() ?? new List<string>()
        }).ToList() ?? new List<TierDto>();
        
        return new LoyaltyProgramDto
        {
            Id = program.Id.Value,
            BrandId = program.BrandId.Value,
            Name = program.Name,
            Description = program.Description,
            Type = program.Type,
            ExpirationPolicy = program.ExpirationPolicy is { } 
                ? new ExpirationPolicyDto
                {
                    ExpirationType = program.ExpirationPolicy.ExpirationType,
                    ExpirationValue = program.ExpirationPolicy.ExpirationValue,
                    ExpirationMonth = program.ExpirationPolicy.ExpirationMonth,
                    ExpirationDay = program.ExpirationPolicy.ExpirationDay,
                    HasExpiration = program.ExpirationPolicy.HasExpiration,
                    ExpiresOnSpecificDate = program.ExpirationPolicy.ExpiresOnSpecificDate
                } 
                : null,
            PointsConfig = program.PointsConfig is { } 
                ? new PointsConfigDto(program.PointsConfig)
                : null,
            TermsAndConditions = program.TermsAndConditions,
            HasTiers = program.HasTiers,
            EnrollmentBonusPoints = program.EnrollmentBonusPoints,
            StartDate = program.StartDate,
            EndDate = program.EndDate,
            IsActive = program.IsActive,
            Rewards = rewards,
            Tiers = tiers
        };
    }

    private RewardDto MapToRewardDto(Reward reward)
    {
        if (reward == null) return null;
        
        return new RewardDto
        {
            Id = reward.Id.ToString(),
            ProgramId = reward.ProgramId.ToString(),
            Title = reward.Title,
            Description = reward.Description,
            RequiredPoints = reward.RequiredValue,
            StartDate = reward.ValidFrom,
            EndDate = reward.ValidTo
        };
    }

    public async Task<OperationResult<Application.DTOs.ProgramAnalyticsDto>> GetProgramAnalyticsAsync()
    {
        try
        {
            // Get all programs
            var programs = await _programRepository.GetAllAsync(0, 1000);
            
            // Calculate analytics
            int totalPrograms = programs.Count();
            int activePrograms = programs.Count(p => p.IsActive);
            int stampPrograms = programs.Count(p => p.Type == LoyaltyProgramType.Stamp);
            int pointsPrograms = programs.Count(p => p.Type == LoyaltyProgramType.Points);
            
            // Get all rewards
            var rewards = new List<Reward>();
            foreach (var program in programs)
            {
                var programRewards = await _programRepository.GetRewardsForProgramAsync(program.Id);
                rewards.AddRange(programRewards);
            }
            
            int totalRewards = rewards.Count;
            int activeRewards = rewards.Count(r => r.IsActive);
            
            var analytics = new Application.DTOs.ProgramAnalyticsDto
            {
                TotalPrograms = totalPrograms,
                ActivePrograms = activePrograms,
                StampPrograms = stampPrograms,
                PointsPrograms = pointsPrograms,
                TotalRewards = totalRewards,
                ActiveRewards = activeRewards,
                ProgramsByBrand = await GetProgramCountByBrandAsync()
            };
            
            return OperationResult<Application.DTOs.ProgramAnalyticsDto>.SuccessResult(analytics);
        }
        catch (Exception ex)
        {
            return OperationResult<Application.DTOs.ProgramAnalyticsDto>.FailureResult($"Failed to get program analytics: {ex.Message}");
        }
    }
    
    private async Task<Dictionary<string, int>> GetProgramCountByBrandAsync()
    {
        var result = new Dictionary<string, int>();
        var brands = await _brandRepository.GetAllAsync(0, 1000);
        
        foreach (var brand in brands)
        {
            var programs = await _programRepository.GetByBrandIdAsync(brand.Id);
            result[brand.Name] = programs.Count();
        }
        
        return result;
    }
    
    public async Task<OperationResult<Application.DTOs.ProgramDetailedAnalyticsDto>> GetProgramDetailedAnalyticsAsync(string programId)
    {
        try
        {
            var programIdObj = new LoyaltyProgramId(Guid.Parse(programId));
            var program = await _programRepository.GetByIdAsync(programIdObj);
            
            if (program == null)
                return OperationResult<Application.DTOs.ProgramDetailedAnalyticsDto>.FailureResult($"Program with ID {programId} not found");
            
            // Get all rewards for this program
            var rewards = await _programRepository.GetRewardsForProgramAsync(programIdObj);
            
            // Get all cards for this program
            var cards = await _unitOfWork.LoyaltyCardRepository.GetByProgramIdAsync(programIdObj);
            
            // Calculate analytics
            int totalCards = cards.Count();
            int activeCards = cards.Count(c => c.Status == CardStatus.Active);
            int suspendedCards = cards.Count(c => c.Status == CardStatus.Suspended);
            int expiredCards = cards.Count(c => c.Status == CardStatus.Expired);
            
            // Get all transactions for this program's cards
            var transactions = new List<Transaction>();
            foreach (var card in cards)
            {
                var cardTransactions = await _unitOfWork.TransactionRepository.GetByCardIdAsync(card.Id);
                transactions.AddRange(cardTransactions);
            }
            
            int totalTransactions = transactions.Count;
            decimal totalPointsIssued = transactions.Where(t => t.Type == TransactionType.PointsIssuance).Sum(t => t.PointsAmount ?? 0);
            decimal totalPointsRedeemed = transactions.Where(t => t.Type == TransactionType.RewardRedemption && t.PointsAmount.HasValue && t.PointsAmount.Value < 0).Sum(t => -1 * (t.PointsAmount ?? 0));
            int totalStampsIssued = transactions.Where(t => t.Type == TransactionType.StampIssuance).Sum(t => t.Quantity ?? 0);
            int totalStampsRedeemed = transactions.Where(t => t.Type == TransactionType.RewardRedemption && t.Quantity.HasValue && t.Quantity.Value < 0).Sum(t => -1 * (t.Quantity ?? 0));
            int totalRedemptions = transactions.Count(t => t.Type == TransactionType.RewardRedemption);
            
            var analytics = new Application.DTOs.ProgramDetailedAnalyticsDto
            {
                ProgramId = program.Id.ToString(),
                ProgramName = program.Name,
                ProgramType = program.Type.ToString(),
                IsActive = program.IsActive,
                TotalCards = totalCards,
                ActiveCards = activeCards,
                SuspendedCards = suspendedCards,
                ExpiredCards = expiredCards,
                TotalRewards = rewards.Count(),
                ActiveRewards = rewards.Count(r => r.IsActive),
                TotalTransactions = totalTransactions,
                TotalPointsIssued = totalPointsIssued,
                TotalPointsRedeemed = totalPointsRedeemed,
                TotalStampsIssued = totalStampsIssued,
                TotalStampsRedeemed = totalStampsRedeemed,
                TotalRedemptions = totalRedemptions
            };
            
            return OperationResult<Application.DTOs.ProgramDetailedAnalyticsDto>.SuccessResult(analytics);
        }
        catch (Exception ex)
        {
            return OperationResult<Application.DTOs.ProgramDetailedAnalyticsDto>.FailureResult($"Failed to get detailed program analytics: {ex.Message}");
        }
    }

    public async Task<OperationResult<PagedResult<LoyaltyProgramDto>>> GetNearbyProgramsAsync(double latitude, double longitude, double radiusKm, int page, int pageSize)
    {
        try
        {
            // Calculate proper pagination parameters
            int skip = (page - 1) * pageSize;
            
            // Use the paginated version of GetAllAsync to avoid the type casting issue
            var programs = await _programRepository.GetAllAsync(skip, pageSize);
            var programDtos = new List<LoyaltyProgramDto>();

            // TODO: In a real implementation, you would:
            // 1. Check if each program's brand has a store with coordinates
            // 2. Calculate distance between user location and store
            // 3. Filter by those within the radius
            // For now, we'll filter to just active programs
            
            foreach (var program in programs)
            {
                // Only include active programs 
                if (program.IsActive)
                {
                    programDtos.Add(MapToDto(program));
                }
            }

            // For accurate total count, we would need a separate count query
            // For now, use approximate counting based on what we have
            int totalCount = programDtos.Count * (page + 1); // Rough estimate for all programs
            
            var result = new PagedResult<LoyaltyProgramDto>(programDtos, totalCount, page, pageSize);

            return OperationResult<PagedResult<LoyaltyProgramDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return OperationResult<PagedResult<LoyaltyProgramDto>>.FailureResult($"Failed to get nearby loyalty programs: {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> DeleteProgramAsync(string id)
    {
        try
        {
            var programId = EntityId.Parse<LoyaltyProgramId>(id);
            var program = await _programRepository.GetByIdAsync(programId);

            if (program == null)
            {
                return OperationResult<bool>.FailureResult($"Loyalty program with ID {id} not found");
            }

            // Since there's no DeleteAsync method, we'll mark it as inactive instead
            // Using reflection as a workaround since IsActive setter is private
            typeof(LoyaltyProgram).GetProperty("IsActive").SetValue(program, false);
            
            await _unitOfWork.BeginTransactionAsync();
            await _programRepository.UpdateAsync(program);
            await _unitOfWork.CommitTransactionAsync();

            return OperationResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<bool>.FailureResult($"Failed to delete loyalty program: {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> RemoveRewardFromProgramAsync(string programId, string rewardId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            var programIdObj = new LoyaltyProgramId(Guid.Parse(programId));
            var rewardIdObj = new RewardId(Guid.Parse(rewardId));
            
            var program = await _programRepository.GetByIdAsync(programIdObj);
            if (program == null)
                return OperationResult<bool>.FailureResult($"Program with ID {programId} not found");
                
            var reward = await _programRepository.GetRewardByIdAsync(rewardIdObj);
            if (reward == null)
                return OperationResult<bool>.FailureResult($"Reward with ID {rewardId} not found");
                
            if (reward.ProgramId != program.Id)
                return OperationResult<bool>.FailureResult("The reward does not belong to the specified program");
                
            // Instead of deleting, deactivate the reward
            reward.Deactivate();
            await _programRepository.UpdateRewardAsync(reward);
            
            await _unitOfWork.CommitTransactionAsync();
            
            return OperationResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<bool>.FailureResult(ex.Message);
        }
    }
}
