using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Application.Services
{
    public class CustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CustomerService() { }
        
        public CustomerService(
            ICustomerRepository customerRepository,
            IStoreRepository storeRepository,
            IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<OperationResult<PagedResult<CustomerDto>>> GetAllAsync(int skip, int limit)
        {
            try
            {
                var customers = await _customerRepository.GetAllAsync(skip, limit);
                var totalCount = await _customerRepository.GetTotalCountAsync();

                var customerDtos = customers.Select(MapToDto).ToList();

                var result = new PagedResult<CustomerDto>(customerDtos, totalCount, skip, limit);

                return OperationResult<PagedResult<CustomerDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperationResult<PagedResult<CustomerDto>>.FailureResult($"Failed to get customers: {ex.Message}");
            }
        }

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

        public async Task<OperationResult<PagedResult<CustomerDto>>> SearchCustomersAsync(string query, int page, int pageSize)
        {
            try
            {
                var customers = await _customerRepository.SearchAsync(query, page, pageSize);
                var totalCount = await _customerRepository.GetTotalCountAsync();

                var customerDtos = new List<CustomerDto>();
                foreach (var customer in customers)
                {
                    customerDtos.Add(MapToDto(customer));
                }

                var result = new PagedResult<CustomerDto>(customerDtos, totalCount, page, pageSize);

                return OperationResult<PagedResult<CustomerDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperationResult<PagedResult<CustomerDto>>.FailureResult($"Failed to search customers: {ex.Message}");
            }
        }

        public async Task<OperationResult<CustomerDto>> CreateCustomerAsync(CreateCustomerDto dto)
        {
            try
            {
                // Validate email format
                if (!IsValidEmail(dto.Email))
                    return OperationResult<CustomerDto>.FailureResult("Invalid email format");
                    
                // Assuming the Customer constructor takes these parameters
                var customer = new Customer(
                    dto.Name,
                    dto.Email,
                    dto.Phone,
                    null,
                    false // marketingConsent default value
                );

                await _unitOfWork.BeginTransactionAsync();
                var newCustomer = await _customerRepository.AddAsync(customer, _unitOfWork.CurrentTransaction);
                await _unitOfWork.CommitTransactionAsync();

                return OperationResult<CustomerDto>.SuccessResult(MapToDto(newCustomer));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return OperationResult<CustomerDto>.FailureResult($"Failed to create customer: {ex.Message}");
            }
        }

        public async Task<OperationResult<CustomerDto>> UpdateCustomerAsync(string id, UpdateCustomerDto dto)
        {
            try
            {
                var customerId = EntityId.Parse<CustomerId>(id);
                var customer = await _customerRepository.GetByIdAsync(customerId);

                if (customer == null)
                    return OperationResult<CustomerDto>.FailureResult($"Customer with ID {id} not found");

                customer.Update(dto.Name, dto.Email, dto.Phone, false);

                await _unitOfWork.BeginTransactionAsync();
                await _customerRepository.UpdateAsync(customer);
                await _unitOfWork.CommitTransactionAsync();

                return OperationResult<CustomerDto>.SuccessResult(MapToDto(customer));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return OperationResult<CustomerDto>.FailureResult($"Failed to update customer: {ex.Message}");
            }
        }

        public async Task<OperationResult<List<CustomerSignupDto>>> GetCustomerSignupsByDateRangeAsync(DateTime start, DateTime end)
        {
            try
            {
                var customers = await _customerRepository.GetBySignupDateRangeAsync(start, end);
                var signups = new List<CustomerSignupDto>();

                foreach (var customer in customers)
                {
                    signups.Add(new CustomerSignupDto
                    {
                        CustomerId = customer.Id.ToString(),
                        Name = customer.Name,
                        Email = customer.Email,
                        SignupDate = customer.CreatedAt
                    });
                }

                return OperationResult<List<CustomerSignupDto>>.SuccessResult(signups);
            }
            catch (Exception ex)
            {
                return OperationResult<List<CustomerSignupDto>>.FailureResult($"Failed to get customer signups: {ex.Message}");
            }
        }

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

        private static CustomerDto MapToDto(Customer customer)
        {
            return new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                CreatedAt = customer.CreatedAt
            };
        }

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
    }

    public class CustomerDto
    {
        public CustomerId Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCustomerDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class UpdateCustomerDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class CustomerSignupDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime SignupDate { get; set; }
    }

    public class StoreDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Address Address { get; set; } 
        public ContactInfo ContactInfo { get; set; } 
        public GeoLocation Location { get; set; }
        public OperatingHours OperatingHours { get; set; }
        public string BrandId { get; set; } = string.Empty;
    }
} 