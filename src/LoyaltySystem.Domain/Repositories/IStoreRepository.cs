using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Domain.Repositories
{
    public interface IStoreRepository
    {
        Task<Store?> GetByIdAsync(StoreId id);
        Task<IEnumerable<Store>> GetAllAsync(int page, int pageSize);
        Task<IEnumerable<Store>> GetByBrandIdAsync(BrandId brandId);
        Task<Store> AddAsync(Store store);
        Task UpdateAsync(Store store);
        Task<IEnumerable<Store>> FindNearbyStoresAsync(double latitude, double longitude, double radiusKm);
        Task<IEnumerable<Transaction>> GetTransactionsAsync(StoreId storeId, DateTime start, DateTime end, int page, int pageSize);
        Task<int> GetTransactionCountAsync(StoreId storeId, DateTime start, DateTime end);
        Task<int> GetTotalStampsIssuedAsync(StoreId storeId, DateTime start, DateTime end);
        Task<decimal> GetTotalPointsIssuedAsync(StoreId storeId, DateTime start, DateTime end);
        Task<int> GetRedemptionCountAsync(StoreId storeId, DateTime start, DateTime end);
    }
} 