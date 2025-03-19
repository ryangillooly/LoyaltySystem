using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Common;
using System.Data;

namespace LoyaltySystem.Domain.Repositories
{
    public interface IStoreRepository
    {
        Task<Store?> GetByIdAsync(StoreId id);
        Task<IEnumerable<Store>> GetAllAsync(int skip = 0, int limit = 50);
        Task<IEnumerable<Store>> GetByBrandIdAsync(BrandId brandId);
        Task<Store> AddAsync(Store store, IDbTransaction transaction = null);
        Task UpdateAsync(Store store);
        Task<IEnumerable<Store>> FindNearbyStoresAsync(double latitude, double longitude, double radiusKm);
        Task<IEnumerable<Transaction>> GetTransactionsAsync(StoreId storeId, DateTime start, DateTime end, int skip = 0, int limit = 50);
        Task<int> GetTransactionCountAsync(StoreId storeId, DateTime start, DateTime end);
        Task<int> GetTotalStampsIssuedAsync(StoreId storeId, DateTime start, DateTime end);
        Task<decimal> GetTotalPointsIssuedAsync(StoreId storeId, DateTime start, DateTime end);
        Task<int> GetRedemptionCountAsync(StoreId storeId, DateTime start, DateTime end);
    }
} 