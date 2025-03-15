using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.DTOs.LoyaltyPrograms;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LoyaltySystem.Application.Services
{
    public class LoyaltyRewardsService : ILoyaltyRewardsService
    {
        private readonly ILoyaltyRewardsRepository _rewardsRepository;
        private readonly ILoyaltyProgramRepository _programRepository;
        private readonly ILogger<LoyaltyRewardsService> _logger;

        public LoyaltyRewardsService(
            ILoyaltyRewardsRepository rewardsRepository,
            ILoyaltyProgramRepository programRepository,
            ILogger<LoyaltyRewardsService> logger)
        {
            _rewardsRepository = rewardsRepository ?? throw new ArgumentNullException(nameof(rewardsRepository));
            _programRepository = programRepository ?? throw new ArgumentNullException(nameof(programRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OperationResult<List<RewardDto>>> GetRewardsByProgramIdAsync(string programId)
        {
            try
            {
                var programIdObj = EntityId.Parse<LoyaltyProgramId>(programId);
                var rewards = await _rewardsRepository.GetByProgramIdAsync(programIdObj);
                var rewardDtos = rewards.Select(MapToDto).ToList();
                
                return OperationResult<List<RewardDto>>.SuccessResult(rewardDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rewards for program {ProgramId}", programId);
                return OperationResult<List<RewardDto>>.FailureResult(ex.Message);
            }
        }

        public async Task<OperationResult<List<RewardDto>>> GetActiveRewardsByProgramIdAsync(string programId)
        {
            try
            {
                var programIdObj = EntityId.Parse<LoyaltyProgramId>(programId);
                var rewards = await _rewardsRepository.GetActiveByProgramIdAsync(programIdObj);
                var rewardDtos = rewards.Select(MapToDto).ToList();
                
                return OperationResult<List<RewardDto>>.SuccessResult(rewardDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active rewards for program {ProgramId}", programId);
                return OperationResult<List<RewardDto>>.FailureResult(ex.Message);
            }
        }

        public async Task<OperationResult<RewardDto>> GetRewardByIdAsync(string rewardId)
        {
            try
            {
                var rewardIdObj = EntityId.Parse<RewardId>(rewardId);
                var reward = await _rewardsRepository.GetByIdAsync(rewardIdObj);
                
                if (reward == null)
                    return OperationResult<RewardDto>.FailureResult($"Reward with ID {rewardId} not found");
                
                return OperationResult<RewardDto>.SuccessResult(MapToDto(reward));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reward {RewardId}", rewardId);
                return OperationResult<RewardDto>.FailureResult(ex.Message);
            }
        }

        public async Task<OperationResult<RewardDto>> CreateRewardAsync(string programId, CreateRewardDto dto)
        {
            try
            {
                var programIdObj = EntityId.Parse<LoyaltyProgramId>(programId);
                var program = await _programRepository.GetByIdAsync(programIdObj);
                
                if (program == null)
                    return OperationResult<RewardDto>.FailureResult($"Program with ID {programId} not found");

                var reward = new Reward(
                    program.Id,
                    dto.Title,
                    dto.Description,
                    dto.RequiredPoints > 0 ? dto.RequiredPoints : dto.RequiredStamps,
                    dto.StartDate,
                    dto.EndDate
                );

                if (!dto.IsActive)
                    reward.Deactivate();

                var createdReward = await _rewardsRepository.AddAsync(reward);
                return OperationResult<RewardDto>.SuccessResult(MapToDto(createdReward));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reward for program {ProgramId}", programId);
                return OperationResult<RewardDto>.FailureResult(ex.Message);
            }
        }

        public async Task<OperationResult<RewardDto>> UpdateRewardAsync(UpdateRewardDto dto)
        {
            try
            {
                var rewardIdObj = EntityId.Parse<RewardId>(dto.Id);
                var reward = await _rewardsRepository.GetByIdAsync(rewardIdObj);
                
                if (reward == null)
                    return OperationResult<RewardDto>.FailureResult($"Reward with ID {dto.Id} not found");

                reward.Update(
                    dto.Title,
                    dto.Description,
                    dto.RequiredPoints,
                    dto.StartDate,
                    dto.EndDate
                );

                if (dto.IsActive)
                    reward.Activate();
                else
                    reward.Deactivate();

                await _rewardsRepository.UpdateAsync(reward);
                return OperationResult<RewardDto>.SuccessResult(MapToDto(reward));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reward {RewardId}", dto.Id);
                return OperationResult<RewardDto>.FailureResult(ex.Message);
            }
        }

        public async Task<OperationResult<bool>> DeleteRewardAsync(string programId, string rewardId)
        {
            try
            {
                var rewardIdObj = EntityId.Parse<RewardId>(rewardId);
                var reward = await _rewardsRepository.GetByIdAsync(rewardIdObj);
                
                if (reward == null)
                    return OperationResult<bool>.FailureResult($"Reward with ID {rewardId} not found");

                var programIdObj = EntityId.Parse<LoyaltyProgramId>(programId);
                if (reward.ProgramId != programIdObj.Value)
                    return OperationResult<bool>.FailureResult("Reward does not belong to the specified program");

                var result = await _rewardsRepository.DeleteAsync(rewardIdObj);
                return OperationResult<bool>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reward {RewardId} from program {ProgramId}", rewardId, programId);
                return OperationResult<bool>.FailureResult(ex.Message);
            }
        }

        public async Task<OperationResult<RewardAnalyticsDto>> GetRewardAnalyticsAsync(string rewardId)
        {
            try
            {
                var rewardIdObj = EntityId.Parse<RewardId>(rewardId);
                var reward = await _rewardsRepository.GetByIdAsync(rewardIdObj);
                
                if (reward == null)
                    return OperationResult<RewardAnalyticsDto>.FailureResult($"Reward with ID {rewardId} not found");

                var analytics = new RewardAnalyticsDto
                {
                    RewardId = rewardId,
                    RewardTitle = reward.Title,
                    TotalRedemptions = await _rewardsRepository.GetTotalRedemptionsAsync(rewardIdObj),
                    RedemptionsLast30Days = await _rewardsRepository.GetRedemptionsInLastDaysAsync(rewardIdObj, 30),
                    RedemptionsLast7Days = await _rewardsRepository.GetRedemptionsInLastDaysAsync(rewardIdObj, 7),
                    AveragePointsPerRedemption = await _rewardsRepository.GetAveragePointsPerRedemptionAsync(rewardIdObj),
                    TotalPointsRedeemed = await _rewardsRepository.GetTotalPointsRedeemedAsync(rewardIdObj),
                    FirstRedemptionDate = await _rewardsRepository.GetFirstRedemptionDateAsync(rewardIdObj),
                    LastRedemptionDate = await _rewardsRepository.GetLastRedemptionDateAsync(rewardIdObj),
                    RedemptionsByStore = await _rewardsRepository.GetRedemptionsByStoreAsync(rewardIdObj),
                    RedemptionsByMonth = await _rewardsRepository.GetRedemptionsByMonthAsync(rewardIdObj)
                };

                return OperationResult<RewardAnalyticsDto>.SuccessResult(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics for reward {RewardId}", rewardId);
                return OperationResult<RewardAnalyticsDto>.FailureResult(ex.Message);
            }
        }

        public async Task<OperationResult<ProgramRewardsAnalyticsDto>> GetProgramRewardsAnalyticsAsync(string programId)
        {
            try
            {
                var programIdObj = EntityId.Parse<LoyaltyProgramId>(programId);
                var program = await _programRepository.GetByIdAsync(programIdObj);
                
                if (program == null)
                    return OperationResult<ProgramRewardsAnalyticsDto>.FailureResult($"Program with ID {programId} not found");

                var analytics = new ProgramRewardsAnalyticsDto
                {
                    ProgramId = programId,
                    ProgramName = program.Name,
                    TotalRewards = await _rewardsRepository.GetTotalRewardsCountAsync(programIdObj),
                    ActiveRewards = await _rewardsRepository.GetActiveRewardsCountAsync(programIdObj),
                    TotalRedemptions = await _rewardsRepository.GetTotalProgramRedemptionsAsync(programIdObj),
                    TotalPointsRedeemed = await _rewardsRepository.GetTotalProgramPointsRedeemedAsync(programIdObj),
                    RedemptionsByRewardType = await _rewardsRepository.GetProgramRedemptionsByTypeAsync(programIdObj),
                    RedemptionsByMonth = await _rewardsRepository.GetProgramRedemptionsByMonthAsync(programIdObj)
                };

                // Get top rewards
                var topRewards = await _rewardsRepository.GetTopRewardsByRedemptionsAsync(programIdObj);
                foreach (var reward in topRewards)
                {
                    var rewardAnalytics = await GetRewardAnalyticsAsync(reward.Id.ToString());
                    if (rewardAnalytics.Success)
                        analytics.TopRewards.Add(rewardAnalytics.Data);
                }

                return OperationResult<ProgramRewardsAnalyticsDto>.SuccessResult(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics for program {ProgramId}", programId);
                return OperationResult<ProgramRewardsAnalyticsDto>.FailureResult(ex.Message);
            }
        }

        public async Task<OperationResult<bool>> CheckRewardEligibility(string rewardId, string loyaltyCardId)
        {
            try
            {
                var rewardIdObj = EntityId.Parse<RewardId>(rewardId);
                var cardIdObj = EntityId.Parse<LoyaltyCardId>(loyaltyCardId);
                
                var reward = await _rewardsRepository.GetByIdAsync(rewardIdObj);
                if (reward == null)
                {
                    return OperationResult<bool>.FailureResult($"Reward with ID {rewardId} not found");
                }
                
                // Check if the reward is active
                if (!reward.IsActive)
                {
                    return OperationResult<bool>.SuccessResult(false);
                }
                
                // Additional eligibility checks would go here...
                // For example, check if the customer has enough points/stamps
                
                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking reward eligibility for reward {RewardId} and card {CardId}", rewardId, loyaltyCardId);
                return OperationResult<bool>.FailureResult(ex.Message);
            }
        }

        public async Task<OperationResult<RewardRedemptionDto>> RedeemReward(RedeemRewardDto redeemDto)
        {
            try
            {
                var rewardIdObj = EntityId.Parse<RewardId>(redeemDto.RewardId);
                var cardIdObj = EntityId.Parse<LoyaltyCardId>(redeemDto.LoyaltyCardId);
                
                // First check eligibility
                var eligibilityResult = await CheckRewardEligibility(redeemDto.RewardId, redeemDto.LoyaltyCardId);
                if (!eligibilityResult.Success || !eligibilityResult.Data)
                {
                    return OperationResult<RewardRedemptionDto>.FailureResult(
                        eligibilityResult.Success ? "Not eligible for reward" : eligibilityResult.Errors.ToString());
                }
                
                // Process the redemption
                // This would typically involve:
                // 1. Create a redemption record
                // 2. Update the loyalty card (subtract points, etc.)
                // 3. Generate a redemption code
                
                var redemption = new RewardRedemptionDto
                {
                    Id = Guid.NewGuid().ToString(),
                    RewardId = redeemDto.RewardId,
                    LoyaltyCardId = redeemDto.LoyaltyCardId,
                    RedemptionCode = Guid.NewGuid().ToString("N"), // Generate a unique code
                    RedeemedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(7), // Example: code valid for 7 days
                    StoreId = redeemDto.StoreId
                };
                
                // Logic to save the redemption would go here
                
                return OperationResult<RewardRedemptionDto>.SuccessResult(redemption);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error redeeming reward for DTO: {@RedeemDto}", redeemDto);
                return OperationResult<RewardRedemptionDto>.FailureResult(ex.Message);
            }
        }

        public async Task<OperationResult<RewardRedemptionDto>> ValidateRedemptionCode(string redemptionCode)
        {
            try
            {
                // Implementation would typically:
                // 1. Look up the redemption code in the database
                // 2. Check if it's valid (not expired, not already used)
                // 3. Return the redemption details
                
                // Placeholder implementation
                if (string.IsNullOrEmpty(redemptionCode))
                    return OperationResult<RewardRedemptionDto>.FailureResult("Redemption code cannot be empty");
                
                // Simulating a database lookup
                // In a real implementation, this would query the database
                
                // For now, we're returning a failure since we don't have actual code validation
                return OperationResult<RewardRedemptionDto>.FailureResult("Code validation not implemented");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating redemption code: {RedemptionCode}", redemptionCode);
                return OperationResult<RewardRedemptionDto>.FailureResult(ex.ToString());
            }
        }

        private static RewardDto MapToDto(Reward reward)
        {
            return new RewardDto
            {
                Id = reward.Id.ToString(),
                ProgramId = reward.ProgramId.ToString(),
                Title = reward.Title,
                Description = reward.Description,
                RequiredPoints = reward.RequiredValue,
                StartDate = reward.ValidFrom,
                EndDate = reward.ValidTo,
                IsActive = reward.IsActive,
                CreatedAt = reward.CreatedAt,
                UpdatedAt = reward.UpdatedAt
            };
        }
    }
}
