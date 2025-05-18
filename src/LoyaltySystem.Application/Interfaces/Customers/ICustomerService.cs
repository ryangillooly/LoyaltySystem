using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Customers;
using LoyaltySystem.Domain.Common;
using System.Data;

namespace LoyaltySystem.Application.Interfaces.Customers;

public interface ICustomerService 
{
    /// <summary>
/// Retrieves a paged list of customers, skipping a specified number and limiting the result count.
/// </summary>
/// <param name="skip">The number of customers to skip before starting to collect the result set.</param>
/// <param name="limit">The maximum number of customers to return.</param>
/// <returns>An operation result containing a paged list of customer data transfer objects.</returns>
Task<OperationResult<PagedResult<CustomerDto>>> GetAllAsync(int skip, int limit);
    /// <summary>
/// Retrieves a customer by their unique identifier.
/// </summary>
/// <param name="id">The unique identifier of the customer.</param>
/// <returns>An operation result containing the customer data if found.</returns>
Task<OperationResult<CustomerDto>> GetCustomerByIdAsync(string id);
    /// <summary>
/// Searches for customers matching the specified query string, returning a paged list of results.
/// </summary>
/// <param name="query">The search term to filter customers by.</param>
/// <param name="page">The page number of results to retrieve.</param>
/// <param name="pageSize">The number of customers per page.</param>
/// <returns>An operation result containing a paged list of matching customer data transfer objects.</returns>
Task<OperationResult<PagedResult<CustomerDto>>> SearchCustomersAsync(string query, int page, int pageSize);
    /// <summary>
/// Adds a new customer using the provided data.
/// </summary>
/// <param name="dto">The data for the customer to be created.</param>
/// <param name="transaction">An optional database transaction for the operation.</param>
/// <returns>An operation result containing the created customer data.</returns>
Task<OperationResult<CustomerDto>> AddCustomerAsync(CreateCustomerDto dto, IDbTransaction? transaction = null);
    /// <summary>
/// Updates the details of an existing customer identified by the specified ID.
/// </summary>
/// <param name="id">The unique identifier of the customer to update.</param>
/// <param name="dto">The data transfer object containing updated customer information.</param>
/// <returns>An operation result containing the updated customer data.</returns>
Task<OperationResult<CustomerDto>> UpdateCustomerAsync(string id, UpdateCustomerDto dto);
    
    
    /// <summary>
/// Retrieves customers who signed up within the specified date range.
/// </summary>
/// <param name="start">The start date of the range (inclusive).</param>
/// <param name="end">The end date of the range (inclusive).</param>
/// <returns>An operation result containing a collection of customers who registered between the given dates.</returns>
Task<OperationResult<IEnumerable<CustomerDto>>> GetCustomerSignupsByDateRangeAsync(DateTime start, DateTime end);
    /// <summary>
/// Retrieves a dictionary mapping age group labels to the number of customers in each group.
/// </summary>
/// <returns>A task that represents the asynchronous operation. The task result contains a dictionary where keys are age group labels and values are customer counts.</returns>
Task<Dictionary<string, int>> GetCustomerAgeGroupsAsync();
    /// <summary>
/// Retrieves the distribution of customers by gender.
/// </summary>
/// <returns>A dictionary mapping gender categories to the number of customers in each category.</returns>
Task<Dictionary<string, int>> GetCustomerGenderDistributionAsync();
    /// <summary>
/// Retrieves the top customer locations ranked by customer count, limited to the specified number.
/// </summary>
/// <param name="limit">The maximum number of locations to return.</param>
/// <returns>A list of key-value pairs where the key is the location and the value is the number of customers in that location.</returns>
Task<List<KeyValuePair<string, int>>> GetTopCustomerLocationsAsync(int limit);
    /// <summary>
/// Asynchronously retrieves the total number of customers in the system.
/// </summary>
/// <returns>The total count of customers.</returns>
Task<int> GetTotalCustomerCountAsync();
    /// <summary>
/// Retrieves the number of customers who possess loyalty cards.
/// </summary>
/// <returns>The count of customers with loyalty cards.</returns>
Task<int> GetCustomersWithCardsCountAsync();
    /// <summary>
/// Finds stores located within a specified radius of the given geographic coordinates.
/// </summary>
/// <param name="latitude">Latitude of the center point.</param>
/// <param name="longitude">Longitude of the center point.</param>
/// <param name="radiusKm">Search radius in kilometers.</param>
/// <returns>An operation result containing a list of nearby stores.</returns>
Task<OperationResult<List<StoreDto>>> FindNearbyStoresAsync(double latitude, double longitude, double radiusKm);
    /// <summary>
/// Links a customer record to an internal user account.
/// </summary>
/// <param name="userId">The unique identifier of the internal user.</param>
/// <param name="customerId">The unique identifier of the customer to link.</param>
/// <returns>An operation result containing the linked internal user data.</returns>
Task<OperationResult<InternalUserDto>> LinkCustomerAsync(UserId userId, string customerId);
}
