using Amazon.DynamoDBv2.Model;
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
        public async Task<Business> CreateBusinessAsync(Business newBusiness)
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

        public async Task<Business> UpdateBusinessAsync(Business updatedBusiness)
        {
            var currentRecord = await _businessRepository.GetBusinessAsync(updatedBusiness.Id);
            if(currentRecord == null) throw new Exception("Record not found.");
            var mergedRecord = Business.Merge(currentRecord, updatedBusiness);
            
            await _businessRepository.UpdateBusinessAsync(mergedRecord);
            
            return mergedRecord;
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

        public async Task<Business> GetBusinessAsync(Guid businessId)
        {
            var business = await _businessRepository.GetBusinessAsync(businessId);
            if (business == null) throw new ResourceNotFoundException("Business not found");
            return business;
        }
        
        public async Task<Campaign> GetCampaignAsync(Guid businessId, Guid campaignId)
        {
            var campaign = await _businessRepository.GetCampaignAsync(businessId, campaignId);
            if (campaign == null) throw new ResourceNotFoundException("Campaign not found");
            return campaign;
        }

        public async Task DeleteBusinessAsync(Guid businessId) => await _businessRepository.DeleteBusinessAsync(businessId);
        public async Task DeleteCampaignAsync(Guid businessId, Guid campaignId) => await _businessRepository.DeleteCampaignAsync(businessId, campaignId);
    }
}