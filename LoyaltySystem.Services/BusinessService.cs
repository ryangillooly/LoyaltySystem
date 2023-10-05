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
        
        // Businesses
        public async Task<Business> CreateBusinessAsync(Business newBusiness)
        {
            var emailExists = await _emailService.IsEmailUnique(newBusiness.ContactInfo.Email);

            if (emailExists)
                throw new InvalidOperationException("Email already exists");
            
            var permissions = new BusinessUserPermissions(newBusiness.Id, newBusiness.OwnerId, UserRole.Owner);

            await _businessRepository.CreateBusinessAsync(newBusiness);
            await _businessRepository.UpdateBusinessUserPermissionsAsync(new List<BusinessUserPermissions> { permissions });
            
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
        public async Task<Business> GetBusinessAsync(Guid businessId)
        {
            var business = await _businessRepository.GetBusinessAsync(businessId);
            if (business == null) throw new ResourceNotFoundException("Business not found");
            return business;
        }
        public async Task DeleteBusinessAsync(Guid businessId) => await _businessRepository.DeleteBusinessAsync(businessId);
        
        
        // Business User Permissions
        public async Task<List<BusinessUserPermissions>> CreateBusinessUserPermissionsAsync(List<BusinessUserPermissions> newBusinessUserPermissions)
        {

            await _businessRepository.CreateBusinessUserPermissionsAsync(newBusinessUserPermissions);
            return newBusinessUserPermissions;
        }

        public async Task<List<BusinessUserPermissions>> UpdateBusinessUsersPermissionsAsync(List<BusinessUserPermissions> updatedBusinessUserPermissions)
        {
            await _businessRepository.UpdateBusinessUserPermissionsAsync(updatedBusinessUserPermissions);
            return updatedBusinessUserPermissions;
        }

        public async Task<List<BusinessUserPermissions>> GetBusinessPermissionsAsync(Guid businessId)
        {
            var businessPermissions = await _businessRepository.GetBusinessPermissionsAsync(businessId);
            if (businessPermissions is null) throw new ResourceNotFoundException($"No Permissions found");
            return businessPermissions;
        }
        public async Task<BusinessUserPermissions> GetBusinessUsersPermissionsAsync(Guid businessId, Guid userId)
        {
            var businessPermissions = await _businessRepository.GetBusinessUsersPermissionsAsync(businessId, userId);
            if (businessPermissions is null) throw new ResourceNotFoundException($"No Permissions found");
            return businessPermissions;
        }
        
        // Campaigns
        public async Task<Campaign> CreateCampaignAsync(Campaign newCampaign) 
        {
            await _businessRepository.CreateCampaignAsync(newCampaign);
            return newCampaign;
        }
        public async Task<IReadOnlyList<Campaign>?> GetAllCampaignsAsync(Guid businessId)
        {
            var campaigns = await _businessRepository.GetAllCampaignsAsync(businessId);
            if (campaigns is null) throw new ResourceNotFoundException("No Campaigns found");
            return campaigns;
        }
        public async Task<Campaign> GetCampaignAsync(Guid businessId, Guid campaignId)
        {
            var campaign = await _businessRepository.GetCampaignAsync(businessId, campaignId);
            if (campaign is null) throw new ResourceNotFoundException("Campaign not found");
            return campaign;
        }
        public async Task<Campaign> UpdateCampaignAsync(Campaign updatedCampaign)
        {
            var currentRecord = await _businessRepository.GetCampaignAsync(updatedCampaign.BusinessId, updatedCampaign.Id);
            if(currentRecord is null) throw new Exception("Record not found.");
            var mergedRecord = Campaign.Merge(currentRecord, updatedCampaign);
            
            await _businessRepository.UpdateCampaignAsync(mergedRecord);
            
            return mergedRecord;
        }
        public async Task DeleteCampaignAsync(Guid businessId, List<Guid> campaignIds) => await _businessRepository.DeleteCampaignAsync(businessId, campaignIds);
    }
}