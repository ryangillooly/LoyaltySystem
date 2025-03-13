using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Domain.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(CustomerId id);
        Task<IEnumerable<Customer>> GetAllAsync(int skip = 0, int limit = 50);
        Task<int> GetTotalCountAsync();
        Task<IEnumerable<Customer>> SearchAsync(string query, int skip = 0, int limit = 50);
        Task<Customer> AddAsync(Customer customer, IDbTransaction transaction = null);
        Task UpdateAsync(Customer customer);
        Task<IEnumerable<Customer>> GetBySignupDateRangeAsync(DateTime start, DateTime end);
        Task<int> GetCustomersWithCardsCountAsync();
        Task<Dictionary<string, int>> GetAgeGroupsAsync();
        Task<Dictionary<string, int>> GetGenderDistributionAsync();
        Task<IEnumerable<KeyValuePair<string, int>>> GetTopLocationsAsync(int limit);
    }
} 