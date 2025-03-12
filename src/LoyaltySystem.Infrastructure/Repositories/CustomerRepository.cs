using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Infrastructure.Data;
using LoyaltySystem.Infrastructure.Data.Extensions;

namespace LoyaltySystem.Infrastructure.Repositories
{
    // Define the CustomerDto for mapping database results
    internal class CustomerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool MarketingConsent { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CustomerRepository : ICustomerRepository
    {
        private readonly IDatabaseConnection _connection;

        public CustomerRepository(IDatabaseConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public async Task<Customer?> GetByIdAsync(CustomerId id)
        {
            const string sql = @"
                SELECT * FROM customers
                WHERE id = @Id";

            // Use the Guid value directly from the EntityId
            // The implicit conversion to Guid is defined in the EntityId base class
            Guid guidId = id; // This uses the implicit operator
            
            var parameters = new { Id = guidId };
            
            var dbConnection = await _connection.GetConnectionAsync();
            var dto = await dbConnection.QuerySingleOrDefaultAsync<CustomerDto>(sql, parameters);
            
            if (dto == null)
                return null;
                
            // Map DTO to domain entity
            var customer = CreateCustomerFromDto(dto);
            
            return customer;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync(int page, int pageSize)
        {
            const string sql = @"
                SELECT * FROM customers
                ORDER BY name
                LIMIT @PageSize OFFSET @Offset";
            
            var parameters = new { Offset = (page - 1) * pageSize, PageSize = pageSize };
            var dbConnection = await _connection.GetConnectionAsync();
            var dtos = await dbConnection.QueryAsync<CustomerDto>(sql, parameters);
            
            // Map DTOs to domain entities
            var customers = new List<Customer>();
            foreach (var dto in dtos)
            {
                var customer = CreateCustomerFromDto(dto);
                customers.Add(customer);
            }
            
            return customers;
        }

        public async Task<int> GetTotalCountAsync()
        {
            const string sql = "SELECT COUNT(*) FROM customers";
            var dbConnection = await _connection.GetConnectionAsync();
            return await dbConnection.ExecuteScalarAsync<int>(sql);
        }

        public async Task<IEnumerable<Customer>> SearchAsync(string searchTerm, int page, int pageSize)
        {
            const string sql = @"
                SELECT * FROM customers
                WHERE name ILIKE @SearchTerm OR email ILIKE @SearchTerm OR phone ILIKE @SearchTerm
                ORDER BY name
                LIMIT @PageSize OFFSET @Offset";
            
            var parameters = new 
            { 
                SearchTerm = $"%{searchTerm}%", 
                Offset = (page - 1) * pageSize, 
                PageSize = pageSize 
            };
            
            var dbConnection = await _connection.GetConnectionAsync();
            var dtos = await dbConnection.QueryAsync<CustomerDto>(sql, parameters);
            
            // Map DTOs to domain entities
            var customers = new List<Customer>();
            foreach (var dto in dtos)
            {
                var customer = CreateCustomerFromDto(dto);
                customers.Add(customer);
            }
            
            return customers;
        }

        public async Task<Customer> AddAsync(Customer customer, IDbTransaction transaction = null)
        {
            // Insert the customer
            const string customerSql = @"
                INSERT INTO customers (id, name, email, phone, marketing_consent, joined_at, last_login_at, created_at, updated_at)
                VALUES (@Id, @Name, @Email, @Phone, @MarketingConsent, @JoinedAt, @LastLoginAt, @CreatedAt, @UpdatedAt)
                RETURNING *";
            
            var dbConnection = await _connection.GetConnectionAsync();
            
            // Use the existing transaction if provided, otherwise create a new one
            bool ownsTransaction = false;
            var localTransaction = transaction;
            
            if (localTransaction == null)
            {
                // Only create a new transaction if one wasn't provided
                localTransaction = await dbConnection.BeginTransactionAsync();
                ownsTransaction = true;
            }
            
            try
            {
                // Determine the ID to use for the database record
                Guid customerId;
                
                if (!string.IsNullOrEmpty(customer.Id.ToString()))
                {
                    // Try to parse the prefixed ID
                    if (EntityId.TryParse<CustomerId>(customer.Id.ToString(), out var parsedId))
                    {
                        customerId = parsedId; // Use the parsed ID
                    }
                    else if (Guid.TryParse(customer.Id.ToString(), out customerId))
                    {
                        // Use the successfully parsed Guid
                    }
                    else
                    {
                        // Generate a new ID if parsing fails
                        customerId = Guid.NewGuid();
                    }
                }
                else
                {
                    // No ID provided, generate a new one
                    customerId = Guid.NewGuid();
                }
                
                // Prepare parameters for the database
                DateTime now = DateTime.UtcNow;
                var customerParams = new
                {
                    Id = customerId,
                    customer.Name,
                    customer.Email,
                    customer.Phone,
                    customer.MarketingConsent,
                    JoinedAt = customer.JoinedAt != default ? customer.JoinedAt : now,
                    LastLoginAt = customer.LastLoginAt,
                    CreatedAt = customer.CreatedAt != default ? customer.CreatedAt : now,
                    UpdatedAt = customer.UpdatedAt != default ? customer.UpdatedAt : now
                };
                
                // Execute the insert and get the inserted record
                var insertedCustomer = await dbConnection.QuerySingleAsync<CustomerDto>(
                    customerSql, 
                    customerParams, 
                    localTransaction);
                    
                // If we created our own transaction (not using an existing one), commit it
                if (ownsTransaction)
                {
                    localTransaction.Commit();
                }
                
                // Create a Customer instance with the data from the inserted record
                // Make sure we use the data from the database to ensure consistency
                var newCustomer = new Customer(
                    insertedCustomer.Name,
                    insertedCustomer.Email,
                    insertedCustomer.Phone,
                    new CustomerId(customerId),
                    insertedCustomer.MarketingConsent);
                
                // IMPORTANT: Set the ID from the database to ensure consistency
                // This ensures the Customer ID matches what's in the database
                string formattedId = $"cus_{FormatGuidToBase64(insertedCustomer.Id)}";
                SetPrivatePropertyValue(newCustomer, "Id", formattedId);
                
                // Set other private properties from the database values
                SetPrivatePropertyValue(newCustomer, "JoinedAt", insertedCustomer.JoinedAt);
                SetPrivatePropertyValue(newCustomer, "LastLoginAt", insertedCustomer.LastLoginAt);
                SetPrivatePropertyValue(newCustomer, "CreatedAt", insertedCustomer.CreatedAt);
                SetPrivatePropertyValue(newCustomer, "UpdatedAt", insertedCustomer.UpdatedAt);
                
                return newCustomer;
            }
            catch (Exception ex)
            {
                // Only rollback if we own the transaction
                if (ownsTransaction && localTransaction != null)
                {
                    localTransaction.Rollback();
                }
                // Add more context to the exception
                throw new Exception($"Error adding customer: {ex.Message}", ex);
            }
            finally
            {
                // Only dispose if we own the transaction
                if (ownsTransaction && localTransaction != null)
                {
                    localTransaction.Dispose();
                }
            }
        }

        // Helper method to set private properties using reflection
        private void SetPrivatePropertyValue<T>(object obj, string propName, T value)
        {
            var prop = obj.GetType().GetProperty(propName);
            if (prop != null)
            {
                prop.SetValue(obj, value, null);
            }
        }

        public async Task<IEnumerable<Customer>> GetNewCustomersAsync(DateTime start, DateTime end)
        {
            const string sql = @"
                SELECT * FROM customers
                WHERE joined_at BETWEEN @Start AND @End
                ORDER BY joined_at DESC";

            var parameters = new { Start = start, End = end };
            var dbConnection = await _connection.GetConnectionAsync();
            var dtos = await dbConnection.QueryAsync<CustomerDto>(sql, parameters);
            
            // Map DTOs to domain entities
            var customers = new List<Customer>();
            foreach (var dto in dtos)
            {
                var customer = CreateCustomerFromDto(dto);
                customers.Add(customer);
            }
            
            return customers;
        }

        // Private helper method to create a Customer entity from a DTO
        private Customer CreateCustomerFromDto(CustomerDto dto)
        {
            var customer = new Customer(
                dto.Name,
                dto.Email,
                dto.Phone,
                new CustomerId(dto.Id),
                dto.MarketingConsent);
                
            // Set private properties
            SetPrivatePropertyValue(customer, "Id", new CustomerId(dto.Id));
            SetPrivatePropertyValue(customer, "JoinedAt", dto.JoinedAt);
            SetPrivatePropertyValue(customer, "LastLoginAt", dto.LastLoginAt);
            SetPrivatePropertyValue(customer, "CreatedAt", dto.CreatedAt);
            SetPrivatePropertyValue(customer, "UpdatedAt", dto.UpdatedAt);
            
            return customer;
        }

        public async Task UpdateAsync(Customer customer)
        {
            const string sql = @"
                UPDATE customers 
                SET name = @Name,
                    email = @Email,
                    phone = @Phone,
                    marketing_consent = @MarketingConsent,
                    last_login_at = @LastLoginAt,
                    updated_at = @UpdatedAt
                WHERE id = @Id";
            
            if (string.IsNullOrEmpty(customer.Id.ToString()))
            {
                throw new ArgumentException("Customer ID cannot be null or empty");
            }
            
            // Try to parse the prefixed customer ID
            Guid customerId;
            
            // First, try to parse as a prefixed ID using CustomerId's parsing
            if (CustomerId.TryParse<CustomerId>(customer.Id.ToString(), out var parsedId))
            {
                // Use the GUID value from parsed ID
                customerId = parsedId;
            }
            else if (!Guid.TryParse(customer.Id.ToString(), out customerId))
            {
                // If both parsing methods fail, throw an exception
                throw new ArgumentException($"Invalid customer ID format: {customer.Id}. Expected a valid prefixed ID or UUID.");
            }
            
            var dbConnection = await _connection.GetConnectionAsync();
            var parameters = new
            {
                Id = customerId,
                customer.Name,
                customer.Email,
                customer.Phone,
                customer.MarketingConsent,
                customer.LastLoginAt,
                customer.UpdatedAt
            };
            
            await dbConnection.ExecuteAsync(sql, parameters);
        }

        public async Task<IEnumerable<Customer>> GetBySignupDateRangeAsync(DateTime start, DateTime end)
        {
            // This method is the same as GetNewCustomersAsync, just with a different name to match the interface
            return await GetNewCustomersAsync(start, end);
        }

        public async Task<int> GetCustomersWithCardsCountAsync()
        {
            const string sql = @"
                SELECT COUNT(DISTINCT c.id) 
                FROM customers c
                JOIN loyalty_cards lc ON c.id = lc.customer_id";
            
            var dbConnection = await _connection.GetConnectionAsync();
            return await dbConnection.ExecuteScalarAsync<int>(sql);
        }

        public async Task<Dictionary<string, int>> GetAgeGroupsAsync()
        {
            // Since we don't have DateOfBirth in the Customer entity, we'll return a simple placeholder
            var result = new Dictionary<string, int>
            {
                { "Unknown", 100 }
            };
            
            return result;
        }

        public async Task<Dictionary<string, int>> GetGenderDistributionAsync()
        {
            // Since we don't have Gender in the Customer entity, we'll return a simple placeholder
            var result = new Dictionary<string, int>
            {
                { "Unknown", 100 }
            };
            
            return result;
        }

        public async Task<IEnumerable<KeyValuePair<string, int>>> GetTopLocationsAsync(int limit)
        {
            // Since we don't have Address in the Customer entity, we'll return a simple placeholder
            var results = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("Unknown", 100)
            };
            
            return results;
        }

        private class AgeGroupResult
        {
            public string AgeGroup { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        private class GenderResult
        {
            public string Gender { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        private class LocationResult
        {
            public string City { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        // Helper method to convert a Guid to the base64 format used in prefixed IDs
        private string FormatGuidToBase64(Guid guid)
        {
            // Convert Guid to URL-safe Base64 string (following the EntityId pattern)
            return Convert.ToBase64String(guid.ToByteArray())
                .Replace("/", "_")
                .Replace("+", "-")
                .Replace("=", "");
        }
    }
} 