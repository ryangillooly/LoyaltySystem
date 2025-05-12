using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Customer;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Application.Interfaces.Customers;

public interface ICustomerService 
{
    Task<OperationResult<PagedResult<CustomerProfileDto>>> GetAllAsync(int skip, int limit);
    Task<OperationResult<CustomerProfileDto>> GetCustomerByIdAsync(string id);
    Task<OperationResult<PagedResult<CustomerProfileDto>>> SearchCustomersAsync(string query, int page, int pageSize);
    Task<OperationResult<CustomerProfileDto>> CreateCustomerAsync(CreateCustomerDto dto);
    Task<OperationResult<CustomerProfileDto>> UpdateCustomerAsync(string id, UpdateCustomerDto dto);
    Task<OperationResult<IEnumerable<Domain.Entities.Customer>>> GetCustomerSignupsByDateRangeAsync(DateTime start, DateTime end);
    Task<Dictionary<string, int>> GetCustomerAgeGroupsAsync();
    Task<Dictionary<string, int>> GetCustomerGenderDistributionAsync();
    Task<List<KeyValuePair<string, int>>> GetTopCustomerLocationsAsync(int limit);
    Task<int> GetTotalCustomerCountAsync();
    Task<int> GetCustomersWithCardsCountAsync();
    Task<OperationResult<List<StoreDto>>> FindNearbyStoresAsync(double latitude, double longitude, double radiusKm);
    Task<OperationResult<UserDto>> LinkCustomerAsync(string userId, string customerId);
}
