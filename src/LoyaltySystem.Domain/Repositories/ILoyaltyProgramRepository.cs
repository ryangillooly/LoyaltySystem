using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Repositories
{
    /// <summary>
    /// Repository interface for the LoyaltyProgram aggregate.
    /// </summary>
    public interface ILoyaltyProgramRepository
    {
        /// <summary>
        /// Gets a loyalty program by its ID.
        /// </summary>
        Task<LoyaltyProgram> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Gets loyalty programs for a specific brand.
        /// </summary>
        Task<IEnumerable<LoyaltyProgram>> GetByBrandIdAsync(Guid brandId);
        
        /// <summary>
        /// Gets active loyalty programs for a specific brand.
        /// </summary>
        Task<IEnumerable<LoyaltyProgram>> GetActiveByBrandIdAsync(Guid brandId);
        
        /// <summary>
        /// Gets loyalty programs by type.
        /// </summary>
        Task<IEnumerable<LoyaltyProgram>> GetByTypeAsync(LoyaltyProgramType type, int skip, int take);
        
        /// <summary>
        /// Adds a new loyalty program.
        /// </summary>
        Task AddAsync(LoyaltyProgram program);
        
        /// <summary>
        /// Updates an existing loyalty program.
        /// </summary>
        Task UpdateAsync(LoyaltyProgram program);
        
        /// <summary>
        /// Gets the rewards for a specific program.
        /// </summary>
        Task<IEnumerable<Reward>> GetRewardsForProgramAsync(Guid programId);
        
        /// <summary>
        /// Gets active rewards for a specific program.
        /// </summary>
        Task<IEnumerable<Reward>> GetActiveRewardsForProgramAsync(Guid programId);
        
        /// <summary>
        /// Gets a specific reward.
        /// </summary>
        Task<Reward> GetRewardByIdAsync(Guid rewardId);
        
        /// <summary>
        /// Adds a new reward.
        /// </summary>
        Task AddRewardAsync(Reward reward);
        
        /// <summary>
        /// Updates an existing reward.
        /// </summary>
        Task UpdateRewardAsync(Reward reward);
    }
} 