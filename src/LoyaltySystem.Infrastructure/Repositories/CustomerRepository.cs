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
                SELECT * FROM Customers
                WHERE Id = @Id";

            var parameters = new { Id = id.Value };
            
            var dbConnection = await _connection.GetConnectionAsync();
            var customer = await dbConnection.QuerySingleOrDefaultAsync<Customer>(sql, parameters);
            
            return customer;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync(int page, int pageSize)
        {
            const string sql = @"
                SELECT * FROM Customers
                ORDER BY Name
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";
            
            var parameters = new { Offset = (page - 1) * pageSize, PageSize = pageSize };
            var dbConnection = await _connection.GetConnectionAsync();
            var customers = await dbConnection.QueryAsync<Customer>(sql, parameters);
            
            return customers;
        }

        public async Task<int> GetTotalCountAsync()
        {
            const string sql = "SELECT COUNT(*) FROM Customers";
            var dbConnection = await _connection.GetConnectionAsync();
            return await dbConnection.ExecuteScalarAsync<int>(sql);
        }

        public async Task<IEnumerable<Customer>> SearchAsync(string searchTerm, int page, int pageSize)
        {
            const string sql = @"
                SELECT * FROM Customers
                WHERE Name LIKE @SearchTerm OR Email LIKE @SearchTerm OR Phone LIKE @SearchTerm
                ORDER BY Name
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";
            
            var parameters = new 
            { 
                SearchTerm = $"%{searchTerm}%", 
                Offset = (page - 1) * pageSize, 
                PageSize = pageSize 
            };
            
            var dbConnection = await _connection.GetConnectionAsync();
            var customers = await dbConnection.QueryAsync<Customer>(sql, parameters);
            
            return customers;
        }

        public async Task<Customer> AddAsync(Customer customer)
        {
            // Insert the customer
            const string customerSql = @"
                INSERT INTO Customers (Id, Name, Email, Phone, MarketingConsent, JoinedAt, LastLoginAt, CreatedAt, UpdatedAt)
                VALUES (@Id, @Name, @Email, @Phone, @MarketingConsent, @JoinedAt, @LastLoginAt, @CreatedAt, @UpdatedAt)
                RETURNING *";
            
            var dbConnection = await _connection.GetConnectionAsync();
            
            using (var transaction = await dbConnection.BeginTransactionAsync())
            {
                try
                {
                    var customerParams = new
                    {
                        customer.Id,
                        customer.Name,
                        customer.Email,
                        customer.Phone,
                        customer.MarketingConsent,
                        customer.JoinedAt,
                        customer.LastLoginAt,
                        customer.CreatedAt,
                        customer.UpdatedAt
                    };
                    
                    var newCustomer = await dbConnection.QuerySingleAsync<Customer>(customerSql, customerParams, transaction);
                    
                    transaction.Commit();
                    return newCustomer;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<IEnumerable<Customer>> GetNewCustomersAsync(DateTime start, DateTime end)
        {
            const string sql = @"
                SELECT * FROM Customers
                WHERE CreatedAt BETWEEN @Start AND @End
                ORDER BY CreatedAt DESC";

            var parameters = new { Start = start, End = end };
            var dbConnection = await _connection.GetConnectionAsync();
            var customers = await dbConnection.QueryAsync<Customer>(sql, parameters);
            
            return customers;
        }

        public async Task UpdateAsync(Customer customer)
        {
            const string sql = @"
                UPDATE Customers 
                SET Name = @Name,
                    Email = @Email,
                    Phone = @Phone,
                    MarketingConsent = @MarketingConsent,
                    LastLoginAt = @LastLoginAt,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";
            
            var dbConnection = await _connection.GetConnectionAsync();
            var parameters = new
            {
                Id = customer.Id,
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
                SELECT COUNT(DISTINCT c.Id) 
                FROM Customers c
                JOIN LoyaltyCards lc ON c.Id = lc.CustomerId";
            
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
    }
} 