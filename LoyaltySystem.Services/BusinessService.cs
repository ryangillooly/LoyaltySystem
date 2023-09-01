using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Interfaces;

namespace LoyaltySystem.Services
{
    public class BusinessService : IBusinessService
    {
        private readonly IRepository<Business> _businessRepository;

        public BusinessService(IRepository<Business> businessRepository)
        {
            _businessRepository = businessRepository;
        }

        public async Task<IEnumerable<Business>> GetAllAsync()
        {
            return await _businessRepository.GetAllAsync();
        }

        public async Task<Business> GetByIdAsync(Guid id)
        {
            return await _businessRepository.GetByIdAsync(id);
        }

        // ... (Other methods like Create, Update, Delete)
    }
}