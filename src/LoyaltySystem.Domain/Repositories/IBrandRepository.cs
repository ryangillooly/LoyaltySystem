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
        
        /// <summary>
        /// Gets all stores for a brand.
        /// </summary>
        Task<IEnumerable<Store>> GetStoresForBrandAsync(Guid brandId);
        
        /// <summary>
        /// Gets a specific store.
        /// </summary>
        Task<Store> GetStoreByIdAsync(Guid storeId);
        
        /// <summary>
        /// Adds a new store.
        /// </summary>
        Task AddStoreAsync(Store store);
        
        /// <summary>
        /// Updates an existing store.
        /// </summary>
        Task UpdateStoreAsync(Store store);
        
        /// <summary>
        /// Gets stores near a location with optional distance constraint.
        /// </summary>
        Task<IEnumerable<Store>> GetStoresNearLocationAsync(double latitude, double longitude, double maxDistanceKm);
    }
} 