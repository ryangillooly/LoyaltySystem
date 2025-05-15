using LoyaltySystem.Application.DTOs.LoyaltyPrograms;
using LoyaltySystem.Application.DTOs.Rewards;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Application.Interfaces;

public interface ILoyaltyRewardsService
{
    Task<OperationResult<List<RewardDto>>> GetRewardsByProgramIdAsync(string programId);
    Task<OperationResult<List<RewardDto>>> GetActiveRewardsByProgramIdAsync(string programId);
    Task<OperationResult<RewardDto>> GetRewardByIdAsync(string rewardId);
    Task<OperationResult<RewardDto>> CreateRewardAsync(string programId, CreateRewardDto dto);
    Task<OperationResult<RewardDto>> UpdateRewardAsync(UpdateRewardDto dto);
    Task<OperationResult<bool>> DeleteRewardAsync(string programId, string rewardId);
    Task<OperationResult<RewardAnalyticsDto>> GetRewardAnalyticsAsync(string rewardId);
    Task<OperationResult<ProgramRewardsAnalyticsDto>> GetProgramRewardsAnalyticsAsync(string programId);
    
    // Customer reward redemption methods
    Task<OperationResult<bool>> CheckRewardEligibility(string rewardId, string loyaltyCardId);
    Task<OperationResult<RewardRedemptionDto>> RedeemReward(RedeemRewardDto redeemDto);
    Task<OperationResult<RewardRedemptionDto>> ValidateRedemptionCode(string redemptionCode);
}
