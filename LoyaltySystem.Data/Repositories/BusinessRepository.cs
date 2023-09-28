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
    
    public async Task CreateBusinessAsync(Business newBusiness)
    {
        var dynamoRecord = _dynamoDbMapper.MapBusinessToItem(newBusiness);
        await _dynamoDbClient.WriteRecordAsync(dynamoRecord, "attribute_not_exists(PK)");
    }
    
     public async Task CreateCampaignAsync(Campaign newCampaign)
    {
        var dynamoRecord = _dynamoDbMapper.MapCampaignToItem(newCampaign);
        await _dynamoDbClient.WriteRecordAsync(dynamoRecord, "attribute_not_exists(PK) AND attribute_not_exists(SK)");
    }
     
    public async Task UpdatePermissionsAsync(List<Permission> permissions)
    {
        foreach (var permission in permissions)
        {
            var dynamoRecord = _dynamoDbMapper.MapPermissionToItem(permission);
            await _dynamoDbClient.WriteRecordAsync(dynamoRecord, "attribute_not_exists(PK) AND attribute_not_exists(SK)");
        }
    }
    
    
   public Task<IEnumerable<Business>> GetAllAsync() => throw new NotImplementedException();

   public async Task<Business> GetByIdAsync(Guid id)
   {
       var response = await _dynamoDbClient.GetBusinessByIdAsync(id);
       
       var location = JsonConvert.DeserializeObject<Location>(response.Item["Location"].S);
       var openingHours = JsonConvert.DeserializeObject<OpeningHours>(response.Item["OpeningHours"].S);
       
       var business = new Business
       {
           Id           = Guid.Parse(response.Item["BusinessId"].S),
           OwnerId      = Guid.Parse(response.Item["OwnerId"].S),
           Name         = response.Item["Name"].S,
           Description  = response.Item["Desc"]?.S,
           Location     = location,
           OpeningHours = openingHours,
           ContactInfo  = new ContactInfo
           {
               Email       = response.Item["Email"].S, 
               PhoneNumber = response.Item["PhoneNumber"].S
           },
           Status =  Enum.Parse<BusinessStatus>(response.Item["Status"].S)
       };

       return business;
   }
   public async Task UpdateAsync(Business entity) => throw new NotImplementedException();

   public async Task DeleteAsync(Guid businessId) => await _dynamoDbClient.DeleteBusinessAsync(businessId);
}