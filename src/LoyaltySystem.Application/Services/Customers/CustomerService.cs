using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Customers;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Application.Interfaces.Customers;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.ValueObjects;
using System.Data;

namespace LoyaltySystem.Application.Services.Customers;

public class CustomerService : ICustomerService 
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUserRepository _userRepository;
    private readonly IStoreRepository _storeRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerService"/> class with required repositories and unit of work.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if any dependency is null.</exception>
    public CustomerService(
        ICustomerRepository customerRepository,
        IUserRepository userRepository,
        IStoreRepository storeRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Retrieves a paginated list of customers.
    /// </summary>
    /// <param name="skip">The number of customers to skip for pagination.</param>
    /// <param name="limit">The maximum number of customers to return.</param>
    /// <returns>An <see cref="OperationResult{T}"/> containing a paged result of <see cref="CustomerDto"/> objects, or a failure result with an error message if retrieval fails.</returns>
    public async Task<OperationResult<PagedResult<CustomerDto>>> GetAllAsync(int skip, int limit)
    {
        try
        {
            var customers = await _customerRepository.GetAllAsync(skip, limit);
            var totalCount = await _customerRepository.GetTotalCountAsync();

            var CustomerProfileDtos = customers.Select(MapToDto).ToList();

            var result = new PagedResult<CustomerDto>(CustomerProfileDtos, totalCount, skip, limit);

            return OperationResult<PagedResult<CustomerDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return OperationResult<PagedResult<CustomerDto>>.FailureResult($"Failed to get customers: {ex.Message}");
        }
    }
    /// <summary>
    /// Retrieves a customer by their unique identifier.
    /// </summary>
    /// <param name="id">The string representation of the customer's ID.</param>
    /// <returns>An <see cref="OperationResult{CustomerDto}"/> containing the customer data if found, or a failure result with an error message.</returns>
    public async Task<OperationResult<CustomerDto>> GetCustomerByIdAsync(string id)
    {
        try
        {
            var customerId = EntityId.Parse<CustomerId>(id);
            var customer = await _customerRepository.GetByIdAsync(customerId);

            return customer == null
                ? OperationResult<CustomerDto>.FailureResult($"Customer with ID {id} not found")
                : OperationResult<CustomerDto>.SuccessResult(MapToDto(customer));

        }
        catch (Exception ex)
        {
            return OperationResult<CustomerDto>.FailureResult($"Failed to get customer: {ex.Message}");
        }
    }
    /// <summary>
    /// Searches for customers matching the specified query and returns a paginated list of customer DTOs.
    /// </summary>
    /// <param name="query">The search term to filter customers.</param>
    /// <param name="page">The page number for pagination.</param>
    /// <param name="pageSize">The number of results per page.</param>
    /// <returns>An <see cref="OperationResult{T}"/> containing a paged result of <see cref="CustomerDto"/> objects that match the search criteria, or a failure result with an error message.</returns>
    public async Task<OperationResult<PagedResult<CustomerDto>>> SearchCustomersAsync(string query, int page, int pageSize)
    {
        try
        {
            var customers = await _customerRepository.SearchAsync(query, page, pageSize);
            var totalCount = await _customerRepository.GetTotalCountAsync();

            var CustomerProfileDtos = new List<CustomerDto>();
            foreach (var customer in customers)
            {
                CustomerProfileDtos.Add(MapToDto(customer));
            }

            var result = new PagedResult<CustomerDto>(CustomerProfileDtos, totalCount, page, pageSize);

            return OperationResult<PagedResult<CustomerDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return OperationResult<PagedResult<CustomerDto>>.FailureResult($"Failed to search customers: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Adds a new customer to the system with email validation and transactional safety.
    /// </summary>
    /// <param name="dto">The data transfer object containing customer creation details.</param>
    /// <param name="transaction">An optional database transaction to use for the operation.</param>
    /// <returns>An <see cref="OperationResult{CustomerDto}"/> indicating success with the created customer data, or failure with an error message.</returns>
    public async Task<OperationResult<CustomerDto>> AddCustomerAsync(CreateCustomerDto dto, IDbTransaction? transaction = null)
    {
        try
        {
            if (!IsValidEmail(dto.Email))
                return OperationResult<CustomerDto>.FailureResult("Invalid email format");

            var customer = MapToCustomer(dto);

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var newCustomer = await _customerRepository.AddAsync(customer, _unitOfWork.CurrentTransaction);
                return OperationResult<CustomerDto>.SuccessResult(MapToDto(newCustomer));
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<CustomerDto>.FailureResult($"Failed to create customer: {ex.Message}");
        }
    }
    /// <summary>
    /// Updates an existing customer's information with the provided data.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to update.</param>
    /// <param name="dto">The data transfer object containing updated customer information.</param>
    /// <returns>An <see cref="OperationResult{CustomerDto}"/> indicating success with the updated customer data, or failure with an error message if the customer is not found or the update fails.</returns>
    public async Task<OperationResult<CustomerDto>> UpdateCustomerAsync(string id, UpdateCustomerDto dto)
    {
        try
        {
            var customerId = EntityId.Parse<CustomerId>(id);
            var customer = await _customerRepository.GetByIdAsync(customerId);

            if (customer == null)
                return OperationResult<CustomerDto>.FailureResult($"Customer with ID {id} not found");

            customer.Update(dto.FirstName, dto.LastName, dto.Username, dto.Email, dto.Phone, dto.Address, false);
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _customerRepository.UpdateAsync(customer);
                return OperationResult<CustomerDto>.SuccessResult(MapToDto(customer));
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<CustomerDto>.FailureResult($"Failed to update customer: {ex.Message}");
        }
    }
    /// <summary>
    /// Retrieves customers who signed up within the specified date range.
    /// </summary>
    /// <param name="start">The start date of the signup range (inclusive).</param>
    /// <param name="end">The end date of the signup range (inclusive).</param>
    /// <returns>An operation result containing a collection of customer DTOs who signed up within the date range.</returns>
    public async Task<OperationResult<IEnumerable<CustomerDto>>> GetCustomerSignupsByDateRangeAsync(DateTime start, DateTime end)
    {
        var customers = await _customerRepository.GetBySignupDateRangeAsync(start, end);
        var customerDtos = customers.Select(CustomerDto.From).ToList();
        return OperationResult<IEnumerable<CustomerDto>>.SuccessResult(customerDtos);
    }
    
    /// <summary>
    /// Retrieves the count of customers grouped by age categories.
    /// </summary>
    /// <returns>A dictionary mapping age group labels to customer counts. Returns an empty dictionary if an error occurs.</returns>
    public async Task<Dictionary<string, int>> GetCustomerAgeGroupsAsync()
    {
        try
        {
            return await _customerRepository.GetAgeGroupsAsync();
        }
        catch (Exception)
        {
            return new Dictionary<string, int>();
        }
    }
    /// <summary>
    /// Retrieves the distribution of customers by gender as a dictionary of gender labels and counts.
    /// </summary>
    /// <returns>A dictionary where keys are gender labels and values are the corresponding customer counts. Returns an empty dictionary if an error occurs.</returns>
    public async Task<Dictionary<string, int>> GetCustomerGenderDistributionAsync()
    {
        try
        {
            return await _customerRepository.GetGenderDistributionAsync();
        }
        catch (Exception)
        {
            return new Dictionary<string, int>();
        }
    }
    /// <summary>
    /// Retrieves the top customer locations ranked by customer count, limited to the specified number.
    /// </summary>
    /// <param name="limit">The maximum number of locations to return.</param>
    /// <returns>A list of key-value pairs where the key is the location and the value is the customer count. Returns an empty list if an error occurs.</returns>
    public async Task<List<KeyValuePair<string, int>>> GetTopCustomerLocationsAsync(int limit)
    {
        try
        {
            var locations = await _customerRepository.GetTopLocationsAsync(limit);
            return new List<KeyValuePair<string, int>>(locations);
        }
        catch (Exception)
        {
            return new List<KeyValuePair<string, int>>();
        }
    }
    /// <summary>
    /// Retrieves the total number of customers.
    /// </summary>
    /// <returns>The total customer count, or 0 if an error occurs.</returns>
    public async Task<int> GetTotalCustomerCountAsync()
    {
        try
        {
            return await _customerRepository.GetTotalCountAsync();
        }
        catch (Exception)
        {
            return 0;
        }
    }
    /// <summary>
    /// Retrieves the number of customers who possess loyalty cards.
    /// </summary>
    /// <returns>The count of customers with loyalty cards, or zero if an error occurs.</returns>
    public async Task<int> GetCustomersWithCardsCountAsync()
    {
        try
        {
            return await _customerRepository.GetCustomersWithCardsCountAsync();
        }
        catch (Exception)
        {
            return 0;
        }
    }
    /// <summary>
    /// Retrieves a list of stores located within a specified radius of the given geographic coordinates.
    /// </summary>
    /// <param name="latitude">Latitude of the center point.</param>
    /// <param name="longitude">Longitude of the center point.</param>
    /// <param name="radiusKm">Search radius in kilometers.</param>
    /// <returns>An <see cref="OperationResult{T}"/> containing a list of <see cref="StoreDto"/> objects representing nearby stores, or a failure result with an error message if the operation fails.</returns>
    public async Task<OperationResult<List<StoreDto>>> FindNearbyStoresAsync(double latitude, double longitude, double radiusKm)
    {
        try
        {
            var stores = await _storeRepository.FindNearbyStoresAsync(latitude, longitude, radiusKm);
            var storeDtos = new List<StoreDto>();

            foreach (var store in stores)
            {
                storeDtos.Add(new StoreDto
                {
                    Id = store.Id.ToString(),
                    Name = store.Name,
                    Address = new Address
                    (
                        store.Address.Line1,
                        store.Address.Line2,
                        store.Address.City,
                        store.Address.State,
                        store.Address.PostalCode,
                        store.Address.Country
                    ),
                    ContactInfo = new ContactInfo
                    (
                        store.ContactInfo.Email,
                        store.ContactInfo.Phone,
                        store.ContactInfo.Website
                    ),
                    Location = new GeoLocation
                    (
                        store.Location.Latitude,
                        store.Location.Longitude
                    ),
                    BrandId = store.Brand.Id.ToString()
                });
            }

            return OperationResult<List<StoreDto>>.SuccessResult(storeDtos);
        }
        catch (Exception ex)
        {
            return OperationResult<List<StoreDto>>.FailureResult($"Failed to find nearby stores: {ex.Message}");
        }
    }
    /// <summary>
        /// Maps a <see cref="CreateCustomerDto"/> to a <see cref="Customer"/> domain entity.
        /// </summary>
        /// <param name="dto">The data transfer object containing customer creation details.</param>
        /// <returns>A new <see cref="Customer"/> entity populated with the provided data.</returns>
        private static Customer MapToCustomer(CreateCustomerDto dto) =>
        new 
        (
            dto.FirstName,
            dto.LastName,
            dto.Username,
            dto.Email,
            dto.Phone,
            dto.Address,
            dto.MarketingConsent
        );
    /// <summary>
        /// Converts a <see cref="Customer"/> domain entity to a <see cref="CustomerDto"/>.
        /// </summary>
        /// <param name="customer">The customer entity to convert.</param>
        /// <returns>A <see cref="CustomerDto"/> representing the customer.</returns>
        private static CustomerDto MapToDto(Customer customer) =>
        new()
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt,
            MarketingConsent = customer.MarketingConsent
        };
    /// <summary>
    /// Determines whether the specified email address is valid according to standard email format rules.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if the email address is valid; otherwise, false.</returns>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Links a user to a customer by assigning the customer ID and customer role to the user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to link.</param>
    /// <param name="customerId">The unique identifier of the customer to link to the user.</param>
    /// <returns>An <see cref="OperationResult{InternalUserDto}"/> indicating success with the updated user data, or failure with an error message if the user or customer is not found, or if the user is already linked.</returns>
    public async Task<OperationResult<InternalUserDto>> LinkCustomerAsync(UserId userId, string customerId)
    {
        var customerIdObj = CustomerId.FromString(customerId);
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return OperationResult<InternalUserDto>.FailureResult("User not found.");

        var customer = await _customerRepository.GetByIdAsync(customerIdObj);
        if (customer == null)
            return OperationResult<InternalUserDto>.FailureResult("Customer not found.");

        if (user.CustomerId != null)
            return OperationResult<InternalUserDto>.FailureResult("User is already linked to a customer.");
        
        user.CustomerId = customerIdObj;
        user.AddRole(RoleType.Customer);
        await _userRepository.UpdateAsync(user);

        return OperationResult<InternalUserDto>.SuccessResult(InternalUserDto.From(user));
    }
}