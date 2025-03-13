using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Domain.Repositories
{
    /// <summary>
    /// Repository interface for the Business aggregate root.
    /// </summary>
    public interface IBusinessRepository
    {
        /// <summary>
        /// Gets a business by its ID.
        /// </summary>
        Task<Business> GetByIdAsync(BusinessId id);

        /// <summary>
        /// Gets all businesses with optional paging.
        /// </summary>
        Task<IEnumerable<Business>> GetAllAsync(int skip = 0, int limit = 50);
        
        /// <summary>
        /// Gets the total count of businesses.
        /// </summary>
        Task<int> GetTotalCountAsync();
        
        /// <summary>
        /// Adds a new business.
        /// </summary>
        /// <param name="business">The business to add</param>
        /// <param name="transaction">Optional transaction to use</param>
        Task<Business> AddAsync(Business business, IDbTransaction transaction = null);
        
        /// <summary>
        /// Updates an existing business.
        /// </summary>
        /// <param name="business">The business to update</param>
        /// <param name="transaction">Optional transaction to use</param>
        Task UpdateAsync(Business business, IDbTransaction transaction = null);
        
        /// <summary>
        /// Deletes a business.
        /// </summary>
        /// <param name="id">The ID of the business to delete</param>
        /// <param name="transaction">Optional transaction to use</param>
        Task DeleteAsync(BusinessId id, IDbTransaction transaction = null);
        
        /// <summary>
        /// Checks if a business with the given name already exists.
        /// </summary>
        Task<bool> ExistsByNameAsync(string name);
        
        /// <summary>
        /// Checks if a business with the given tax ID already exists.
        /// </summary>
        Task<bool> ExistsByTaxIdAsync(string taxId);
        
        /// <summary>
        /// Gets businesses by name (partial match).
        /// </summary>
        Task<IEnumerable<Business>> SearchByNameAsync(string nameQuery, int page = 1, int pageSize = 20);
        
        /// <summary>
        /// Gets all brands for a business.
        /// </summary>
        Task<IEnumerable<Brand>> GetBrandsForBusinessAsync(BusinessId businessId);
    }
} 