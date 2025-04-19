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
            // Explicitly select columns and use PascalCase aliases matching the DTO
            const string sql = @"
                SELECT 
                    id AS Id, 
                    prefixed_id AS PrefixedId, 
                    first_name AS FirstName, 
                    last_name AS LastName, 
                    username AS UserName, 
                    email AS Email, 
                    phone AS Phone, 
                    marketing_consent AS MarketingConsent, 
                    created_at AS CreatedAt, 
                    updated_at AS UpdatedAt 
                FROM customers 
                WHERE id = @Id";
                
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

        public async Task<Customer> AddAsync(Customer customer, IDbTransaction? transaction = null)
        {
            var dbConnection = await _dbConnection.GetConnectionAsync();

            try
            {
                // Generate and assign the PrefixedId before inserting
                var customerId = new CustomerId(customer.Id.Value); // Assuming base Entity<T> has Value property
                customer.PrefixedId = customerId.ToString();

                await dbConnection.ExecuteAsync(@"
                    INSERT INTO 
                        customers 
                            (id, prefixed_id, first_name, last_name, username, email, phone, marketing_consent, created_at, updated_at)
                        VALUES 
                            (@CustomerId, @PrefixedId, @FirstName, @LastName, @UserName, @Email, @Phone, @MarketingConsent, @CreatedAt, @UpdatedAt)
                    ",
                    new
                    {
                        CustomerId = customer.Id.Value,
                        customer.PrefixedId, // Add the new property
                        customer.FirstName,
                        customer.LastName,
                        customer.UserName,
                        customer.Email,
                        customer.Phone,
                        customer.MarketingConsent,
                        customer.CreatedAt,
                        customer.UpdatedAt
                    },
                    transaction
                );
                
                return customer;
            }
            catch (Exception ex)
            {
                // Consider using a proper logging framework like Serilog or Microsoft.Extensions.Logging
                Console.WriteLine($"Error adding customer: {ex.Message}");
                throw; // Re-throw the exception to allow higher layers to handle it
            }
        }
                
        public async Task<IEnumerable<Customer>> SearchAsync(string searchTerm, int skip = 0, int limit = 50)
        {
            const string sql = @"
                SELECT 
                    * 
                FROM 
                    customers
                WHERE 
                    first_name ILIKE @SearchTerm OR
                    last_name ILIKE @SearchTerm OR
                    email ILIKE @SearchTerm OR 
                    phone ILIKE @SearchTerm
                ORDER BY 
                    first_name, 
                    last_name
                LIMIT 
                    @Limit 
                OFFSET 
                    @Skip";
            
            var parameters = new 
            { 
                SearchTerm = $"%{searchTerm}%", 
                Skip = skip, 
                Limit = limit 
            };
            
            var dbConnection = await _dbConnection.GetConnectionAsync();
            var dtos = await dbConnection.QueryAsync<CustomerDbObjectDto>(sql, parameters);
            
            // Map DTOs to domain entities
            return dtos.Select(CreateCustomerFromDto).ToList();
        }

        public async Task<IEnumerable<Customer>> GetBySignupDateRangeAsync(DateTime start, DateTime end)
        {
            const string sql = @"
                SELECT 
                    * 
                FROM 
                    customers
                WHERE 
                    joined_at BETWEEN @Start AND @End
                ORDER BY 
                    joined_at DESC";

            var parameters = new { Start = start, End = end };
            var dbConnection = await _dbConnection.GetConnectionAsync();
            var dtos = await dbConnection.QueryAsync<CustomerDbObjectDto>(sql, parameters);
            
            // Map DTOs to domain entities
            return dtos.Select(CreateCustomerFromDto).ToList();
        }

        
        // Private helper method to create a Customer entity from a DTO
        private static Customer CreateCustomerFromDto(CustomerDbObjectDto dbObjectDto) =>
            // Use the constructor that requires names
            new Customer(
                dbObjectDto.FirstName ?? string.Empty, // Handle potential DB nulls
                dbObjectDto.LastName ?? string.Empty,  // Handle potential DB nulls
                dbObjectDto.UserName ?? string.Empty, // Username might be null from DB if SELECT *
                dbObjectDto.Email ?? string.Empty,    // Handle potential DB nulls
                dbObjectDto.Phone ?? string.Empty,    // Handle potential DB nulls
                null, // Address not selected/mapped here
                dbObjectDto.MarketingConsent,
                new CustomerId(dbObjectDto.Id) // Pass the ID
            )
            {
                // Set properties not handled by this constructor (if any)
                CreatedAt = dbObjectDto.CreatedAt,
                UpdatedAt = dbObjectDto.UpdatedAt
            };

        public async Task UpdateAsync(Customer customer)
        {
            const string sql = @"
                UPDATE 
                    customers 
                SET 
                    first_name = @FirstName,
                    last_name = @LastName,          
                    username = @UserName,
                    email = @Email,
                    phone = @Phone,
                    marketing_consent = @MarketingConsent,
                    updated_at = @UpdatedAt
                WHERE 
                    id = @Id";
            
            if (string.IsNullOrEmpty(customer.Id.ToString()))
                throw new ArgumentException("Customer ID cannot be null or empty");
            
            Guid customerId;
            if (EntityId.TryParse<CustomerId>(customer.Id.ToString(), out var parsedId))
            {
                customerId = parsedId;
            }
            else if (!Guid.TryParse(customer.Id.ToString(), out customerId))
            {
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
                customer.UpdatedAt
            };
            
            await dbConnection.ExecuteAsync(sql, parameters);
        }
        
        public async Task<int> GetCustomersWithCardsCountAsync()
        {
            const string sql = @"
                SELECT 
                    COUNT(DISTINCT c.id) 
                FROM 
                    customers c INNER JOIN 
                    loyalty_cards lc ON c.id = lc.customer_id";
            
            var dbConnection = await _dbConnection.GetConnectionAsync();
            return await dbConnection.ExecuteScalarAsync<int>(sql);
        }

        public async Task<Dictionary<string, int>> GetAgeGroupsAsync()
        {
            // TODO: Since we don't have DateOfBirth in the Customer entity, we'll return a simple placeholder
            var result = new Dictionary<string, int>
            {
                { "Unknown", 100 }
            };
            
            return result;
        }

        public async Task<Dictionary<string, int>> GetGenderDistributionAsync()
        {
            // TODO: Since we don't have Gender in the Customer entity, we'll return a simple placeholder
            var result = new Dictionary<string, int>
            {
                { "Unknown", 100 }
            };
            
            return result;
        }

        public async Task<IEnumerable<KeyValuePair<string, int>>> GetTopLocationsAsync(int limit)
        {
            // TODO:  Since we don't have Address in the Customer entity, we'll return a simple placeholder
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

        #region Private DTO and Helper Methods
        
        /// <summary>
        /// Internal DTO class to map database results.
        /// Properties should match column names (Dapper handles snake_case -> PascalCase).
        /// </summary>
        private class CustomerDbObjectDto
        {
            public Guid Id { get; set; } // Maps to id
            public string PrefixedId { get; set; } = string.Empty; // Maps to prefixed_id
            public string FirstName { get; set; } = string.Empty; // Maps to first_name
            public string LastName { get; set; } = string.Empty; // Maps to last_name
            public string UserName { get; set; } = string.Empty; // Maps to username
            public string Email { get; set; } = string.Empty; // Maps to email
            public string Phone { get; set; } = string.Empty; // Maps to phone
            public bool MarketingConsent { get; set; } // Maps to marketing_consent
            public DateTime CreatedAt { get; set; } // Maps to created_at
            public DateTime? UpdatedAt { get; set; } // Maps to updated_at
            // Add other columns from SELECT * if needed (e.g., address related?)
        }
        #endregion
    }
} 