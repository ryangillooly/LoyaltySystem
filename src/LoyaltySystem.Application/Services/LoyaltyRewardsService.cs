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
