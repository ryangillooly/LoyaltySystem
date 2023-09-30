using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Settings;
using LoyaltySystem.Data.Clients;
using Newtonsoft.Json;

namespace LoyaltySystem.Data.Repositories;

public class LoyaltyCardRepository : ILoyaltyCardRepository
{
    private readonly IDynamoDbClient _dynamoDbClient;
    private readonly IDynamoDbMapper _dynamoDbMapper;

    public LoyaltyCardRepository(IDynamoDbClient dynamoDbClient, IDynamoDbMapper dynamoDbMapper) =>
        (_dynamoDbClient, _dynamoDbMapper) = (dynamoDbClient, dynamoDbMapper);
    public async Task CreateLoyaltyCardAsync(LoyaltyCard newLoyaltyCard)
    {
        var dynamoRecord = _dynamoDbMapper.MapLoyaltyCardToItem(newLoyaltyCard);
        await _dynamoDbClient.WriteRecordAsync(dynamoRecord, "attribute_not_exists(PK) AND attribute_not_exists(SK)");
    }

    public async Task<Redemption> RedeemRewardAsync(Redemption redemption) => throw new NotImplementedException();
    public async Task<IEnumerable<LoyaltyCard>> GetAllAsync() => throw new NotImplementedException();
    
    public async Task<LoyaltyCard?> GetLoyaltyCardAsync(Guid userId, Guid businessId)
    {
        var response = await _dynamoDbClient.GetLoyaltyCardAsync(userId, businessId);

        if (response is null) return null;
        
        return new LoyaltyCard(userId, businessId)
        {
            Id              = Guid.Parse(response.Item["CardId"].S),
            Points          = Convert.ToInt32(response.Item["Points"].N),
            DateIssued      = Convert.ToDateTime(response.Item["DateIssued"].S),
            DateLastStamped = Convert.ToDateTime(response.Item["LastStampDate"].S),
            Status          = Enum.Parse<LoyaltyStatus>(response.Item["Status"].S)
        };
    }

    public async Task UpdateLoyaltyCardAsync(LoyaltyCard updatedLoyaltyCard)
    {
        var dynamoRecord = _dynamoDbMapper.MapLoyaltyCardToItem(updatedLoyaltyCard);
        await _dynamoDbClient.UpdateRecordAsync(dynamoRecord, null);
    }
    public async Task DeleteLoyaltyCardAsync(Guid userId, Guid businessId) => await _dynamoDbClient.DeleteLoyaltyCardAsync(userId, businessId);
    
    
}