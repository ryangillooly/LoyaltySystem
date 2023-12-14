using LoyaltySystem.Core.DTOs;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Mappers;

namespace LoyaltySystem.Services
{
    public class BusinessService : IBusinessService
    {
        private readonly IBusinessRepository _businessRepository;
        private readonly IEmailService       _emailService;
        
        public BusinessService(IBusinessRepository businessRepository, IEmailService emailService) 
            => (_businessRepository, _emailService) = (businessRepository, emailService);
        
        // Businesses
        public async Task<Business> CreateBusinessAsync(CreateBusinessDto dto)
        {
            var newBusiness     = new BusinessMapper().CreateBusinessFromCreateBusinessDto(dto);
            var emailExists = await _emailService.IsEmailUnique(newBusiness.ContactInfo.Email);
            var permissions     = new BusinessUserPermissions(newBusiness.Id, newBusiness.OwnerId, UserRole.Owner);
            var token           = new BusinessEmailToken(newBusiness.Id, newBusiness.ContactInfo.Email);
            
            if (emailExists) throw new InvalidOperationException($"Email {newBusiness.ContactInfo.Email} already exists");
            
            try
            {
                // TODO: Need to make sure that we are only creating a business if the Owning User is Active
                await _businessRepository.CreateBusinessAsync(newBusiness, permissions, token);
                await _emailService.SendVerificationEmailAsync(newBusiness, token);
               //  await _businessRepository.UpdateBusinessUserPermissionsAsync(new List<BusinessUserPermissions> { permissions });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            return newBusiness;
        }
        public async Task<Business> UpdateBusinessAsync(Business updatedBusiness)
        {
            var currentRecord = await _businessRepository.GetBusinessAsync(updatedBusiness.Id);
            var mergedRecord = Business.Merge(currentRecord, updatedBusiness);
            
            await _businessRepository.UpdateBusinessAsync(mergedRecord);
            
            return mergedRecord;
        }
        public async Task<Business> GetBusinessAsync(Guid businessId) =>
            await _businessRepository.GetBusinessAsync(businessId);
        public async Task<List<Business>> GetBusinessesAsync(List<Guid> businessIdList) =>
            await _businessRepository.GetBusinessesAsync(businessIdList);
        public async Task DeleteBusinessAsync(Guid businessId)
        {
            // TODO: 
            await _businessRepository.DeleteBusinessAsync(businessId);
        }
        public async Task VerifyEmailAsync(VerifyBusinessEmailDto dto) => await _businessRepository.VerifyEmailAsync(dto);


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
        public async Task<List<BusinessUserPermissions>> GetBusinessPermissionsAsync(Guid businessId) =>
            await _businessRepository.GetBusinessPermissionsAsync(businessId);
        public async Task<BusinessUserPermissions> GetBusinessUsersPermissionsAsync(Guid businessId, Guid userId) =>
            await _businessRepository.GetBusinessUsersPermissionsAsync(businessId, userId);

        public async Task DeleteBusinessUsersPermissionsAsync(Guid businessId, List<Guid> userIdList) =>
            await _businessRepository.DeleteUsersPermissionsAsync(businessId, userIdList);
        
        // Campaigns
        public async Task<Campaign> CreateCampaignAsync(Campaign newCampaign) 
        {
            await _businessRepository.CreateCampaignAsync(newCampaign);
            return newCampaign;
        }
        public async Task<IReadOnlyList<Campaign>?> GetAllCampaignsAsync(Guid businessId) =>
            await _businessRepository.GetAllCampaignsAsync(businessId);
        public async Task<Campaign> GetCampaignAsync(Guid businessId, Guid campaignId) =>
            await _businessRepository.GetCampaignAsync(businessId, campaignId);
        public async Task<Campaign> UpdateCampaignAsync(Campaign updatedCampaign)
        {
            var currentRecord = await _businessRepository.GetCampaignAsync(updatedCampaign.BusinessId, updatedCampaign.Id);
            var mergedRecord = Campaign.Merge(currentRecord, updatedCampaign);
            
            await _businessRepository.UpdateCampaignAsync(mergedRecord);
            
            return mergedRecord;
        }
        public async Task DeleteCampaignAsync(Guid businessId, List<Guid> campaignIds) => 
            await _businessRepository.DeleteCampaignAsync(businessId, campaignIds);
    }
}