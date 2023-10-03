using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Data.Clients;
using Newtonsoft.Json;

namespace LoyaltySystem.Data.Repositories;

public class BusinessRepository : IBusinessRepository
{
    private readonly IDynamoDbClient _dynamoDbClient;
    private readonly IDynamoDbMapper _dynamoDbMapper;

    public BusinessRepository(IDynamoDbClient dynamoDbClient, IDynamoDbMapper dynamoDbMapper) =>
        (_dynamoDbClient, _dynamoDbMapper) = (dynamoDbClient, dynamoDbMapper);
    
    // Businesses
    public async Task CreateBusinessAsync(Business newBusiness)
    {
        var dynamoRecord = _dynamoDbMapper.MapBusinessToItem(newBusiness);
        await _dynamoDbClient.WriteRecordAsync(dynamoRecord, "attribute_not_exists(PK)");
    }
    public async Task<Business?> GetBusinessAsync(Guid businessId)
    {
        var response = await _dynamoDbClient.GetBusinessAsync(businessId);
       
        if (response is null) return null;
       
        return new Business
        {
            Id           = Guid.Parse(response.Item["BusinessId"].S),
            OwnerId      = Guid.Parse(response.Item["OwnerId"].S),
            Name         = response.Item["Name"].S,
            Description  = response.Item["Desc"]?.S,
            Location     = JsonConvert.DeserializeObject<Location>(response.Item["Location"].S),
            OpeningHours = JsonConvert.DeserializeObject<OpeningHours>(response.Item["OpeningHours"].S),
            ContactInfo  = new ContactInfo
            {
                Email       = response.Item["Email"].S, 
                PhoneNumber = response.Item["PhoneNumber"].S
            },
            Status =  Enum.Parse<BusinessStatus>(response.Item["Status"].S)
        };
    }
    public async Task UpdateBusinessAsync(Business updatedBusiness)
    {
        var dynamoRecord = _dynamoDbMapper.MapBusinessToItem(updatedBusiness);
        await _dynamoDbClient.UpdateRecordAsync(dynamoRecord, null);
    }
    public async Task DeleteBusinessAsync(Guid businessId) => await _dynamoDbClient.DeleteItemsWithPkAsync($"Business#{businessId}");

    
    // Business User Permissions
    public async Task CreateBusinessUserPermissionsAsync(BusinessUserPermissions newBusinessUserPermissions)
    {
        foreach (var permission in newBusinessUserPermissions.Permissions)
        {
            // This needs to be changed to a BatchWriteItem request, to make use of batching, and limit DDB calls
            var dynamoRecord = _dynamoDbMapper.MapBusinessUserPermissionsToItem(newBusinessUserPermissions);
            await _dynamoDbClient.WriteRecordAsync(dynamoRecord, "attribute_not_exists(PK) AND attribute_not_exists(SK)");
        }
    }
    
    /*
    public async Task UpdateBusinessUserPermissionsAsync(List<BusinessUserPermission> updatedBusinessUserPermissions)
    {
        // This needs to be changed to a BatchWriteItem request, to make use of batching, and limit DDB calls
        foreach (var permission in updatedBusinessUserPermissions)
        {
            var dynamoRecord = _dynamoDbMapper.MapBusinessUserPermissionsToItem(permission);
            await _dynamoDbClient.WriteRecordAsync(dynamoRecord, "attribute_not_exists(PK) AND attribute_not_exists(SK)");
        }
    }
    */
    
   // Campaigns
   public async Task CreateCampaignAsync(Campaign newCampaign)
   {
       var dynamoRecord = _dynamoDbMapper.MapCampaignToItem(newCampaign);
       await _dynamoDbClient.WriteRecordAsync(dynamoRecord, "attribute_not_exists(PK) AND attribute_not_exists(SK)");
   }
   public async Task<IReadOnlyList<Campaign>?> GetAllCampaignsAsync(Guid businessId)
   {
       var response = await _dynamoDbClient.GetAllCampaignsAsync(businessId);
       
       if (response is null) return null;

       var campaignList = new List<Campaign>();

       foreach (var item in response.Items)
       {
           var campaign = new Campaign
           {
               Id         = Guid.Parse(item["CampaignId"].S),
               BusinessId = Guid.Parse(item["BusinessId"].S),
               Name       = item["Name"].S,
               StartTime  = DateTime.Parse(item["StartTime"].S),
               EndTime    = DateTime.Parse(item["EndTime"].S),
               IsActive   = item["IsActive"].BOOL
           };

           var settings = new JsonSerializerSettings
           {
               MissingMemberHandling = MissingMemberHandling.Ignore
           };
           
           var rewards = JsonConvert.DeserializeObject<List<Reward>>(item["Rewards"].S, settings);

           campaign.Rewards = rewards;
           
           campaignList.Add(campaign);
       }

       return campaignList;
   }
   public async Task<Campaign?> GetCampaignAsync(Guid businessId, Guid campaignId)
   {
       var response = await _dynamoDbClient.GetCampaignAsync(businessId, campaignId);
       
       if (response is null) return null;
       
      return new Campaign
       {
           Id         = Guid.Parse(response.Item["CampaignId"].S),
           BusinessId = Guid.Parse(response.Item["BusinessId"].S),
           Name       = response.Item["Name"].S,
           Rewards    = JsonConvert.DeserializeObject<List<Reward>>(response.Item["Rewards"].S),
           StartTime  = DateTime.Parse(response.Item["StartTime"].S),
           EndTime    = DateTime.Parse(response.Item["EndTime"].S),
           IsActive   = response.Item["IsActive"].BOOL
       };
   }
   public async Task UpdateCampaignAsync(Campaign updatedCampaign)
   {
       var dynamoRecord = _dynamoDbMapper.MapCampaignToItem(updatedCampaign);
       await _dynamoDbClient.UpdateRecordAsync(dynamoRecord, null);
   }
   public async Task DeleteCampaignAsync(Guid businessId, List<Guid> campaignIds) => await _dynamoDbClient.DeleteCampaignAsync(businessId, campaignIds);

}