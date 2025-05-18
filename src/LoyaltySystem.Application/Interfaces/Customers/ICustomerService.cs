using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Customers;
using LoyaltySystem.Domain.Common;
using System.Data;

namespace LoyaltySystem.Application.Interfaces.Customers;

public interface ICustomerService 
{
    Task<OperationResult<PagedResult<CustomerDto>>> GetAllAsync(int skip, int limit);
    Task<OperationResult<CustomerDto>> GetCustomerByIdAsync(string id);
    Task<OperationResult<PagedResult<CustomerDto>>> SearchCustomersAsync(string query, int page, int pageSize);
    Task<OperationResult<CustomerDto>> AddCustomerAsync(CreateCustomerDto dto, IDbTransaction? transaction = null);
    Task<OperationResult<CustomerDto>> UpdateCustomerAsync(string id, UpdateCustomerDto dto);
    
    
    Task<OperationResult<IEnumerable<CustomerDto>>> GetCustomerSignupsByDateRangeAsync(DateTime start, DateTime end);
    Task<Dictionary<string, int>> GetCustomerAgeGroupsAsync();
    Task<Dictionary<string, int>> GetCustomerGenderDistributionAsync();
    Task<List<KeyValuePair<string, int>>> GetTopCustomerLocationsAsync(int limit);
    Task<int> GetTotalCustomerCountAsync();
    Task<int> GetCustomersWithCardsCountAsync();
    Task<OperationResult<List<StoreDto>>> FindNearbyStoresAsync(double latitude, double longitude, double radiusKm);
    Task<OperationResult<InternalUserDto>> LinkCustomerAsync(UserId userId, string customerId);
}
