using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Application.Interfaces;

public interface IStoreService {
    Task<OperationResult<StoreDto>> GetStoreByIdAsync(string id);
    Task<OperationResult<PagedResult<StoreDto>>> GetAllStoresAsync(int skip, int limit);
    Task<OperationResult<List<StoreDto>>> GetStoresByBrandIdAsync(string brandId);
    Task<OperationResult<StoreDto>> CreateStoreAsync(CreateStoreDto dto);
    Task<OperationResult<StoreDto>> UpdateStoreAsync(string id, UpdateStoreDto dto);
    Task<OperationResult<PagedResult<TransactionDto>>> GetStoreTransactionsAsync(string storeId, DateTime start, DateTime end, int page, int pageSize);
    Task<OperationResult<StoreStatsDto>> GetStoreStatsAsync(string storeId, DateTime start, DateTime end);
    Task<OperationResult<OperatingHoursDto>> GetStoreOperatingHoursAsync(string storeId);
    Task<int> GetTransactionCountAsync(string storeId, DateTime start, DateTime end);
    Task<int> GetTotalStampsIssuedAsync(string storeId, DateTime start, DateTime end);
    Task<decimal> GetTotalPointsIssuedAsync(string storeId, DateTime start, DateTime end);
    Task<int> GetRedemptionCountAsync(string storeId, DateTime start, DateTime end);
}
