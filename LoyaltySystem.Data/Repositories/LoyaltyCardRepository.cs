using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Enums;
using static LoyaltySystem.Core.Exceptions.LoyaltyCardExceptions;
using static LoyaltySystem.Core.Exceptions.BusinessExceptions;
using static LoyaltySystem.Core.Exceptions.UserExceptions;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Settings;
using static LoyaltySystem.Core.Convertors;

namespace LoyaltySystem.Data.Repositories;

public class LoyaltyCardRepository : ILoyaltyCardRepository
{
    private readonly IDynamoDbClient _dynamoDbClient;
    private readonly IDynamoDbMapper _dynamoDbMapper;
    private readonly DynamoDbSettings _dynamoDbSettings;

    public LoyaltyCardRepository(IDynamoDbClient dynamoDbClient, IDynamoDbMapper dynamoDbMapper, DynamoDbSettings dynamoDbSettings) =>
        (_dynamoDbClient, _dynamoDbMapper, _dynamoDbSettings) = (dynamoDbClient, dynamoDbMapper, dynamoDbSettings);
    
    // Constants
    private const string UserPrefix     = "User#";
    private const string BusinessPrefix = "Business#";
    private const string CardPrefix     = "Card#";
    private const string MetaUser       = "Meta#UserInfo";
    private const string MetaBusiness   = "Meta#BusinessInfo";

    public async Task CreateLoyaltyCardAsync(LoyaltyCard newLoyaltyCard)
    {
        var transactGetRequest = BuildLoyaltyCardGetItemsRequest(newLoyaltyCard.UserId, newLoyaltyCard.BusinessId);
        var transactGetResponse = await _dynamoDbClient.TransactGetItemsAsync(transactGetRequest);
        ValidateTransactGetResponse(transactGetResponse, newLoyaltyCard.UserId, newLoyaltyCard.BusinessId);
        await InsertLoyaltyCardRecord(newLoyaltyCard);
    }
    public async Task<Redemption> RedeemRewardAsync(Redemption redemption) => throw new NotImplementedException(); // TODO: Implement
    public async Task<IEnumerable<LoyaltyCard>> GetLoyaltyCardsAsync(Guid userId)
    {
        var queryRequest = new QueryRequest
        {
            TableName = _dynamoDbSettings.TableName,
            KeyConditionExpression = "PK = :PKValue AND begins_with(SK, :SKValue)",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":PKValue", new AttributeValue { S = UserPrefix + userId } },
                { ":SKValue", new AttributeValue { S = "Card" } }
            }
        };
        var response = await _dynamoDbClient.QueryAsync(queryRequest);
        if (response.Items.Count == 0) throw new NoCardsFoundException(userId);

        
        return response.Items.Select(item => item.ConvertFromDynamoItemToLoyaltyCard()).ToList();
    }
    public async Task<LoyaltyCard?> GetLoyaltyCardAsync(Guid userId, Guid businessId)
    {
        var getRequest = new GetItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {"PK", new AttributeValue {S = UserPrefix + userId}},
                {"SK", new AttributeValue {S = CardPrefix + BusinessPrefix + businessId}}
            }
        };
        
        var response = await _dynamoDbClient.GetItemAsync(getRequest);
        if (response.Item.Count == 0 || !response.IsItemSet) throw new CardNotFoundException(userId, businessId);

        return response.Item.ConvertFromDynamoItemToLoyaltyCard();
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
    }
    public async Task DeleteLoyaltyCardAsync(Guid userId, Guid businessId)
    {
        var deleteRequest = new DeleteItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {"PK", new AttributeValue {S = UserPrefix + userId}},
                {"SK", new AttributeValue {S = CardPrefix + BusinessPrefix + businessId}}
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
                    UpdateExpression = "SET Points = :points, LastRedeemDate = :lastRedeemDate",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        {":points",         new AttributeValue  { N = loyaltyRecord["Points"].N} },
                        {":lastRedeemDate", new AttributeValue { S = loyaltyRecord["LastRedeemDate"].S} }
                    }
                }
            }
        };

        await _dynamoDbClient.TransactWriteItemsAsync(transactWriteItems);
    }
    
    
    // Helpers
    private async Task InsertLoyaltyCardRecord(LoyaltyCard newLoyaltyCard)
    {
        var dynamoRecord = _dynamoDbMapper.MapLoyaltyCardToItem(newLoyaltyCard);
        var putRequest = new PutItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Item = dynamoRecord,
            ConditionExpression = "attribute_not_exists(PK) AND attribute_not_exists(SK)"
        };
        try
        {
            await _dynamoDbClient.PutItemAsync(putRequest);
        }
        catch(ConditionalCheckFailedException)
        {
            throw new LoyaltyCardAlreadyExistsException(newLoyaltyCard.UserId, newLoyaltyCard.BusinessId);
        }
    }
    private TransactGetItemsRequest BuildLoyaltyCardGetItemsRequest(Guid userId, Guid businessId)
    {
        return new TransactGetItemsRequest
        {
            TransactItems = new List<TransactGetItem>
            {
                BuildTransactGetItem(UserPrefix + userId, MetaUser),
                BuildTransactGetItem(BusinessPrefix + businessId, MetaBusiness)
            }
        };
    }
    private TransactGetItem BuildTransactGetItem(string pkValue, string skValue)
    {
        return new TransactGetItem
        {
            Get = new Get
            {
                TableName = _dynamoDbSettings.TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "PK", new AttributeValue { S = pkValue } },
                    { "SK", new AttributeValue { S = skValue } }
                }
            }
        };
    }
    private void ValidateTransactGetResponse(TransactGetItemsResponse response, Guid userId, Guid businessId)
    {
        var user     = ConvertFromDynamoItemToUser(response.Responses.First().Item);
        var business = ConvertFromDynamoItemToBusiness(response.Responses.Last().Item);

        if (user     is null)       throw new UserNotFoundException(userId);
        if (business is null)       throw new BusinessNotFoundException(businessId);
        if (user.IsNotActive())     throw new UserNotActiveException(userId, Enum.Parse<UserStatus>(user.Status.ToString()));
        if (business.IsNotActive()) throw new BusinessNotActiveException(businessId, Enum.Parse<BusinessStatus>(business.Status.ToString()));
    }

}