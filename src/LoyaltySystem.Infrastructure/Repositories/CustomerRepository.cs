using System.Data;
using Dapper;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Infrastructure.Data;
using LoyaltySystem.Application.DTOs;

namespace LoyaltySystem.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly IDatabaseConnection _dbConnection;

        public CustomerRepository(IDatabaseConnection connection) =>
            _dbConnection = connection ?? throw new ArgumentNullException(nameof(connection));

        public async Task<Customer?> GetByIdAsync(CustomerId id)
        {
            const string sql = "SELECT * FROM customers WHERE id = @Id";
            var parameters = new { Id = id.Value };
            
            var dbConnection = await _dbConnection.GetConnectionAsync();
            var dto = await dbConnection.QuerySingleOrDefaultAsync<CustomerDbObjectDto>(sql, parameters);

            return dto is { }
                ? CreateCustomerFromDto(dto)
                : null;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync(int skip = 0, int limit = 50)
        {
            var parameters = new { Skip = skip, Limit = limit };
            var dbConnection = await _dbConnection.GetConnectionAsync();
            var dtos = await dbConnection.QueryAsync<CustomerDbObjectDto>(
                """
                    SELECT * 
                    FROM customers
                    ORDER BY first_name, last_name
                    LIMIT @Limit 
                    OFFSET @Skip
                """, 
                parameters
            );
            
            // Map DTOs to domain entities
            return dtos.Select(CreateCustomerFromDto).ToList();
        }

        public async Task<int> GetTotalCountAsync()
        {
            var dbConnection = await _dbConnection.GetConnectionAsync();
            return await dbConnection.ExecuteScalarAsync<int>(
                """
                    SELECT COUNT(*) 
                    FROM customers
                """
            );
        }

        public async Task<Customer> AddAsync(Customer customer, IDbTransaction transaction = null)
        {
            var dbConnection = await _dbConnection.GetConnectionAsync();
        
            // Track whether we created the transaction or are using an existing one
            bool ownsTransaction = transaction == null;
        
            // If no transaction was provided, create one
            // TODO: Check this transaction logic. We are using "ExecuteInTransaction" over these calls, so i assume we can remove this ?
            transaction ??= dbConnection.BeginTransaction();
            
            try
            {
                await dbConnection.ExecuteAsync(@"
                    INSERT INTO 
                        customers 
                            (id, first_name, last_name, username, email, phone, marketing_consent, last_login_at, created_at, updated_at)
                        VALUES 
                            (@CustomerId, @FirstName, @LastName, @UserName, @Email, @Phone, @MarketingConsent, @LastLoginAt, @CreatedAt, @UpdatedAt)
                    ",
                    new
                    {
                        CustomerId = customer.Id.Value,
                        customer.FirstName,
                        customer.LastName,
                        customer.UserName,
                        customer.Email,
                        customer.Phone,
                        customer.MarketingConsent,
                        customer.LastLoginAt,
                        customer.CreatedAt,
                        customer.UpdatedAt
                    },
                    transaction
                );
                
                if (ownsTransaction)
                    transaction.Commit();
                
                return customer;
            }
            catch (Exception ex)
            {
                if (ownsTransaction)
                    transaction.Rollback();
                
                throw new Exception($"Error adding customer: {ex.Message}", ex);
            }
            finally
            {
                if (ownsTransaction && transaction != null)
                    transaction.Dispose();
            }
        }
                
        public async Task<IEnumerable<Customer>> SearchAsync(string searchTerm, int skip = 0, int limit = 50)
        {
            const string sql = @"
                SELECT * 
                FROM customers
                WHERE 
                    first_name ILIKE @SearchTerm OR
                    last_name ILIKE @SearchTerm OR
                    email ILIKE @SearchTerm OR 
                    phone ILIKE @SearchTerm
                ORDER BY 
                    first_name, 
                    last_name
                LIMIT @Limit 
                OFFSET @Skip";
            
            var parameters = new 
            { 
                SearchTerm = $"%{searchTerm}%", 
                Skip = skip, 
                Limit = limit 
            };
            
            var dbConnection = await _dbConnection.GetConnectionAsync();
            var dtos = await dbConnection.QueryAsync<CustomerDbObjectDto>(sql, parameters);
            
            // Map DTOs to domain entities
            var customers = new List<Customer>();
            foreach (var dto in dtos)
            {
                var customer = CreateCustomerFromDto(dto);
                customers.Add(customer);
            }
            
            return customers;
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

        public async Task<IEnumerable<Customer>> GetBySignupDateRangeAsync(DateTime start, DateTime end)
        {
            const string sql = @"
                SELECT * FROM customers
                WHERE joined_at BETWEEN @Start AND @End
                ORDER BY joined_at DESC";

            var parameters = new { Start = start, End = end };
            var dbConnection = await _dbConnection.GetConnectionAsync();
            var dtos = await dbConnection.QueryAsync<CustomerDbObjectDto>(sql, parameters);
            
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
        private static Customer CreateCustomerFromDto(CustomerDbObjectDto dbObjectDto) =>
            new ()
            {
                Id = new CustomerId(dbObjectDto.Id),
                FirstName = dbObjectDto.FirstName,
                LastName = dbObjectDto.LastName,
                Email = dbObjectDto.Email,
                Phone = dbObjectDto.Phone,
                MarketingConsent = dbObjectDto.MarketingConsent,
                LastLoginAt = dbObjectDto.LastLoginAt,
                CreatedAt = dbObjectDto.CreatedAt,
                UpdatedAt = dbObjectDto.UpdatedAt
            };

        public async Task UpdateAsync(Customer customer)
        {
            const string sql = @"
                UPDATE customers 
                SET first_name = @FirstName,
                    last_name = @LastName,          
                    username = @UserName,
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
            
            var dbConnection = await _dbConnection.GetConnectionAsync();
            var parameters = new
            {
                Id = customerId,
                customer.FirstName,
                customer.LastName,
                customer.UserName,
                customer.Email,
                customer.Phone,
                customer.MarketingConsent,
                customer.LastLoginAt,
                customer.UpdatedAt
            };
            
            await dbConnection.ExecuteAsync(sql, parameters);
        }
        
        public async Task<int> GetCustomersWithCardsCountAsync()
        {
            const string sql = @"
                SELECT COUNT(DISTINCT c.id) 
                FROM customers c
                JOIN loyalty_cards lc ON c.id = lc.customer_id";
            
            var dbConnection = await _dbConnection.GetConnectionAsync();
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