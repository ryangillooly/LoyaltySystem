using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Enums;
using static LoyaltySystem.Core.Exceptions.LoyaltyCardExceptions;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Settings;

namespace LoyaltySystem.Data.Repositories;

public class LoyaltyCardRepository : ILoyaltyCardRepository
{
    private readonly IDynamoDbClient _dynamoDbClient;
    private readonly IDynamoDbMapper _dynamoDbMapper;
    private readonly DynamoDbSettings _dynamoDbSettings;

    public LoyaltyCardRepository(IDynamoDbClient dynamoDbClient, IDynamoDbMapper dynamoDbMapper, DynamoDbSettings dynamoDbSettings) =>
        (_dynamoDbClient, _dynamoDbMapper, _dynamoDbSettings) = (dynamoDbClient, dynamoDbMapper, dynamoDbSettings);
    
    public async Task CreateLoyaltyCardAsync(LoyaltyCard newLoyaltyCard)
    {
        var dynamoRecord = _dynamoDbMapper.MapLoyaltyCardToItem(newLoyaltyCard);
        var putRequest = new PutItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Item = dynamoRecord,
            ConditionExpression = "attribute_not_exists(PK) AND attribute_not_exists(SK)"
        };
        await _dynamoDbClient.PutItemAsync(putRequest);
    }
    
    public async Task<Redemption> RedeemRewardAsync(Redemption redemption) => throw new NotImplementedException();
    public async Task<IEnumerable<LoyaltyCard>> GetAllAsync() => throw new NotImplementedException();
    public async Task<LoyaltyCard?> GetLoyaltyCardAsync(Guid userId, Guid businessId)
    {
        var getRequest = new GetItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {"PK", new AttributeValue {S = $"User#{userId}"}},
                {"SK", new AttributeValue {S = $"Card#Business#{businessId}"}}
            }
        };
        
        var response = await _dynamoDbClient.GetItemAsync(getRequest);
        
        if (response.Item.Count == 0 || !response.IsItemSet)
            throw new CardNotFoundException(userId, businessId);
        
        return new LoyaltyCard(userId, businessId)
        {
            Id              = Guid.Parse(response.Item["CardId"].S),
            Points          = Convert.ToInt32(response.Item["Points"].N),
            IssueDate       = Convert.ToDateTime(response.Item["IssueDate"].S),
            LastStampedDate = Convert.ToDateTime(response.Item["LastStampDate"].S),
            Status          = Enum.Parse<LoyaltyStatus>(response.Item["Status"].S)
        };
    }

    public async Task UpdateLoyaltyCardAsync(LoyaltyCard updatedLoyaltyCard)
    {
        var dynamoRecord = _dynamoDbMapper.MapLoyaltyCardToItem(updatedLoyaltyCard);

        var updateRequest = new UpdateItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {"PK", new AttributeValue { S = dynamoRecord["PK"].S }},
                {"SK", new AttributeValue { S = dynamoRecord["SK"].S }}
            },
            UpdateExpression = "SET #St = :status, LastUpdatedDate = :lastUpdatedDate",  // Using the alias #St for Status
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                {":status",          new AttributeValue { S = dynamoRecord["Status"].S }},
                {":lastUpdatedDate", new AttributeValue { S = updatedLoyaltyCard.LastUpdatedDate.ToString() }}
            },
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                {"#St", "Status"}  // Mapping the alias #St to the actual attribute name "Status"
            }
        };


        await _dynamoDbClient.UpdateItemAsync(updateRequest);
        //await _dynamoDbClient.UpdateRecordAsync(dynamoRecord, null);
    }

    public async Task DeleteLoyaltyCardAsync(Guid userId, Guid businessId)
    {
        var deleteRequest = new DeleteItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {"PK", new AttributeValue {S = $"User#{userId}"}},
                {"SK", new AttributeValue {S = $"Card#Business#{businessId}"}}
            }
        };

        await _dynamoDbClient.DeleteItemAsync(deleteRequest);
    }




    public async Task StampLoyaltyCardAsync(LoyaltyCard loyaltyCard)
    {
        var stampRecord   =  _dynamoDbMapper.MapLoyaltyCardToStampItem(loyaltyCard);
        var loyaltyRecord = _dynamoDbMapper.MapLoyaltyCardToItem(loyaltyCard);

        var transactWriteItems = new List<TransactWriteItem>
        {
            new ()
            {
                Put = new Put
                {
                    TableName = _dynamoDbSettings.TableName,
                    Item = stampRecord,
                    ConditionExpression = "attribute_not_exists(PK) AND attribute_not_exists(SK)"
                }
            },
            new ()
            {
                Update = new Update
                {
                    TableName = _dynamoDbSettings.TableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                         {"PK", new AttributeValue {S = loyaltyRecord["PK"].S}},
                         {"SK", new AttributeValue {S = loyaltyRecord["SK"].S}}
                    },
                    UpdateExpression = "SET Points = :points, LastStampDate = :lastStampDate",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        {":points",        new AttributeValue {N = loyaltyRecord["Points"].N}},
                        {":lastStampDate", new AttributeValue {S = $"{DateTime.UtcNow}"}}
                    }
                }
            }
        };

        await _dynamoDbClient.TransactWriteItemsAsync(transactWriteItems);
    }
    public async Task RedeemLoyaltyCardRewardAsync(LoyaltyCard loyaltyCard, Guid campaignId, Guid rewardId)
    {
        var redeemAction  = _dynamoDbMapper.MapLoyaltyCardToRedeemItem(loyaltyCard, campaignId, rewardId);
        var loyaltyRecord = _dynamoDbMapper.MapLoyaltyCardToItem(loyaltyCard);
        
        var transactWriteItems = new List<TransactWriteItem>
        {
            new ()
            {
                Put = new Put
                {
                    TableName = _dynamoDbSettings.TableName,
                    Item = redeemAction,
                    ConditionExpression = "attribute_not_exists(PK) AND attribute_not_exists(SK)"
                }
            },
            new ()
            {
                Update = new Update
                {
                    TableName = _dynamoDbSettings.TableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        {"PK", new AttributeValue {S = loyaltyRecord["PK"].S}},
                        {"SK", new AttributeValue {S = loyaltyRecord["SK"].S}}
                    },
                    UpdateExpression = "SET Points = :points, LastRedeemDate = :lastStampDate",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        {":points",        new AttributeValue  { N = loyaltyRecord["Points"].N} },
                        {":lastRedeemDate", new AttributeValue { S = loyaltyRecord["LastRedeemDate"].S} }
                    }
                }
            }
        };

        await _dynamoDbClient.TransactWriteItemsAsync(transactWriteItems);
    }
}