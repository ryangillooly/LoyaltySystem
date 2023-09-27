using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Settings;
using LoyaltySystem.Data.Clients;

namespace LoyaltySystem.Data.Repositories;

public class BusinessRepository : IBusinessRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDbSettings _dynamoDbSettings;
    private readonly IDynamoDbClient _dynamoDbClient;
    private readonly IDynamoDbMapper _dynamoDbMapper;

    public BusinessRepository(IAmazonDynamoDB dynamoDb, 
        DynamoDbSettings dynamoDbSettings,
        IDynamoDbClient dynamoDbClient,
        IDynamoDbMapper dynamoDbMapper)
    {
        _dynamoDb         = dynamoDb;
        _dynamoDbSettings = dynamoDbSettings;
        _dynamoDbClient = dynamoDbClient;
        _dynamoDbMapper = dynamoDbMapper;
    }
    
    public async Task CreateBusinessAsync(Business newBusiness)
    {
        var dynamoRecord = _dynamoDbMapper.MapBusinessToItem(newBusiness);
        await _dynamoDbClient.WriteRecord(dynamoRecord, "attribute_not_exists(PK)");
    }

    /*
     public async Task<Campaign> CreateCampaignAsync(Campaign newCampaign)
    {
        
    }
    */
    public async Task UpdatePermissionsAsync(List<Permission> permissions)
    {
        foreach (var permission in permissions)
        {
            var dynamoRecord = _dynamoDbMapper.MapPermissionToItem(permission);
            _dynamoDbClient.WriteRecord(dynamoRecord, "attribute_not_exists(PK) AND attribute_not_exists(SK)");
        }
    }
    
   public Task<IEnumerable<Business>> GetAllAsync() => throw new NotImplementedException();
   public Task<Business> GetByIdAsync(Guid id) => throw new NotImplementedException();
   public Task UpdateAsync(Business entity) => throw new NotImplementedException();
   public Task DeleteAsync(Guid id) => throw new NotImplementedException();
}