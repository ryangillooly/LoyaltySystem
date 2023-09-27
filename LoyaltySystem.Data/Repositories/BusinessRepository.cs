using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Data.Clients;

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
        await _dynamoDbClient.WriteRecord(dynamoRecord, "attribute_not_exists(PK)");
    }
    
     public async Task CreateCampaignAsync(Campaign newCampaign)
    {
        var dynamoRecord = _dynamoDbMapper.MapCampaignToItem(newCampaign);
        await _dynamoDbClient.WriteRecord(dynamoRecord, "attribute_not_exists(PK) AND attribute_not_exists(SK)");
    }
     
    public async Task UpdatePermissionsAsync(List<Permission> permissions)
    {
        foreach (var permission in permissions)
        {
            var dynamoRecord = _dynamoDbMapper.MapPermissionToItem(permission);
            await _dynamoDbClient.WriteRecord(dynamoRecord, "attribute_not_exists(PK) AND attribute_not_exists(SK)");
        }
    }
    
    
   public Task<IEnumerable<Business>> GetAllAsync() => throw new NotImplementedException();
   public Task<Business> GetByIdAsync(Guid id) => throw new NotImplementedException();
   public Task UpdateAsync(Business entity) => throw new NotImplementedException();
   public Task DeleteAsync(Guid id) => throw new NotImplementedException();
}