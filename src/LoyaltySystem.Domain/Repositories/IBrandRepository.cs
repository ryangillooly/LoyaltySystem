using LoyaltySystem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Domain.Entities;
using System.Data;

namespace LoyaltySystem.Domain.Repositories
{
    /// <summary>
    /// Repository interface for the Brand aggregate.
    /// </summary>
    public interface IBrandRepository
    {
        /// <summary>
        /// Gets a brand by its ID.
        /// </summary>
        Task<Brand> GetByIdAsync(BrandId id);
        
        /// <summary>
        /// Gets all brands with optional paging.
        /// </summary>
        Task<IEnumerable<Brand>> GetAllAsync(int skip = 0, int limit = 50);
        
        /// <summary>
        /// Gets brands by category.
        /// </summary>
        Task<IEnumerable<Brand>> GetByCategoryAsync(string category, int skip = 0, int take = 50);

        /// <summary>
        /// Checks if a brand with the given name already exists.
        /// </summary>
        Task<bool> ExistsByNameAsync(string name);

        /// <summary>
        /// Adds a new brand.
        /// </summary>
        Task<Brand> AddAsync(Brand brand, IDbTransaction transaction = null);
        
        /// <summary>
        /// Updates an existing brand.
        /// </summary>
        Task UpdateAsync(Brand brand);
    }
} 