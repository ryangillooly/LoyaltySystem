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
        private readonly IEmailService       _emailService;
        
        public BusinessService(IBusinessRepository businessRepository, IAuditService auditService, IEmailService emailService) 
            => (_businessRepository, _auditService, _emailService) = (businessRepository, auditService, emailService);

        public async Task<Business> CreateAsync(Business newBusiness)
        {
            var emailExists = await _emailService.IsEmailUnique(newBusiness.ContactInfo.Email);

            if (emailExists)
                throw new InvalidOperationException("Email already exists");

            var auditRecord = new AuditRecord(EntityType.Business, newBusiness.Id, ActionType.CreateAccount)
            {
                Source = "Mobile Webpage"
            };
            
            var permission = new Permission
            {
                UserId     = newBusiness.OwnerId,
                BusinessId = newBusiness.Id,
                Role       = UserRole.Owner
            };
            
            await _businessRepository.CreateBusinessAsync(newBusiness);
            await _businessRepository.UpdatePermissionsAsync(new List<Permission>{permission});
            await _auditService.CreateAuditRecordAsync<Business>(auditRecord);
            
            return newBusiness;
        }

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