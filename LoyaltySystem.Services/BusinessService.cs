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
        private readonly IEmailService       _emailService;
        
        public BusinessService(IBusinessRepository businessRepository, IEmailService emailService) 
            => (_businessRepository, _emailService) = (businessRepository, emailService);

        public async Task<Business> CreateAsync(Business newBusiness)
        {
            var emailExists = await _emailService.IsEmailUnique(newBusiness.ContactInfo.Email);

            if (emailExists)
                throw new InvalidOperationException("Email already exists");

            var permission = new Permission
            {
                UserId     = newBusiness.OwnerId,
                BusinessId = newBusiness.Id,
                Role       = UserRole.Owner
            };
            
            await _businessRepository.CreateBusinessAsync(newBusiness);
            await _businessRepository.UpdatePermissionsAsync(new List<Permission>{permission});
            
            return newBusiness;
        }
        public async Task UpdatePermissionsAsync(List<Permission> permissions)
        {
            await _businessRepository.UpdatePermissionsAsync(permissions);
        }
        public async Task<Campaign> CreateCampaignAsync(Campaign newCampaign) 
        {
            await _businessRepository.CreateCampaignAsync(newCampaign);
            return newCampaign;
        }

        public async Task<IEnumerable<Business>> GetAllAsync() => await _businessRepository.GetAllAsync();
        public async Task<Business> GetByIdAsync(Guid businessId) => await _businessRepository.GetByIdAsync(businessId);
        public async Task DeleteAsync(Guid businessId) => await _businessRepository.DeleteAsync(businessId);
    }
}