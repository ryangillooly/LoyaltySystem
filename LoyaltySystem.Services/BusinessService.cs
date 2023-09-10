using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Interfaces;

namespace LoyaltySystem.Services
{
    public class BusinessService : IBusinessService
    {
        private readonly IRepository<Business> _businessRepository;
        public BusinessService(IRepository<Business> businessRepository) => _businessRepository = businessRepository;
        
        public async Task<Business> CreateAsync(Business newBusiness) => await _businessRepository.AddAsync(newBusiness);
        public async Task<IEnumerable<Business>> GetAllAsync() => await _businessRepository.GetAllAsync();
        public async Task<Business> GetByIdAsync(Guid id) => await _businessRepository.GetByIdAsync(id);
    }
}