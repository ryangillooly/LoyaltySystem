using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Interfaces;

namespace LoyaltySystem.Services
{
    public class BusinessService : IBusinessService
    {
        private readonly IBusinessRepository _businessRepository;
        private readonly IAuditService       _auditService;
        
        public BusinessService(IBusinessRepository businessRepository, IAuditService auditService) 
            => (_businessRepository, _auditService) = (businessRepository, auditService);
        
        public async Task<Business> CreateAsync(Business newBusiness) => await _businessRepository.AddAsync(newBusiness);
        
        public async Task UpdatePermissionsAsync(List<Permission> permissions)
        {
            await _businessRepository.UpdatePermissionsAsync(permissions);

            foreach(var permission in permissions)
            {
                var auditRecord = new AuditRecord(EntityType.User, permission.UserId, ActionType.PermissionsAltered);
                await _auditService.CreateAuditRecordAsync<Permission>(auditRecord);
            }
        }
        
        public async Task<IEnumerable<Business>> GetAllAsync() => await _businessRepository.GetAllAsync();
        public async Task<Business> GetByIdAsync(Guid id) => await _businessRepository.GetByIdAsync(id);
    }
}