using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Settings;

namespace LoyaltySystem.Data.Repositories;

public class LoyaltyCardRepository : ILoyaltyCardRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDbSettings _dynamoDbSettings;

    public LoyaltyCardRepository(IAmazonDynamoDB dynamoDb, DynamoDbSettings dynamoDbSettings)
    {
        _dynamoDb         = dynamoDb;
        _dynamoDbSettings = dynamoDbSettings;
    }
    
    public async Task<LoyaltyCard> CreateAsync(LoyaltyCard newLoyaltyCard)
    {
        var userItem = new Dictionary<string, AttributeValue>
        {
            // New PK and SK patterns
            { "PK",          new AttributeValue { S = $"User#{newLoyaltyCard.UserId}" }},
            { "SK",          new AttributeValue { S = $"Business#{newLoyaltyCard.BusinessId}" }},
            
            // New Type attribute
            { "CardId",        new AttributeValue { S = $"{newLoyaltyCard.Id}" }},
            { "BusinessId",    new AttributeValue { S = $"{newLoyaltyCard.BusinessId}" }},
            { "UserId",        new AttributeValue { S = $"{newLoyaltyCard.UserId}" }},
            { "EntityType",    new AttributeValue { S = newLoyaltyCard.GetType().Name }},
            { "Points",        new AttributeValue { N = $"{newLoyaltyCard.Points}" }},
            { "DateIssued",    new AttributeValue { S = $"{newLoyaltyCard.DateIssued}" }},
            { "LastStampDate", new AttributeValue { S = $"{newLoyaltyCard.DateLastStamped}" }},
            { "Status",        new AttributeValue { S = $"{newLoyaltyCard.Status}" }},
            
            { "BusinessLoyaltyList-PK",  new AttributeValue { S = $"{newLoyaltyCard.BusinessId}" }},
            { "BusinessLoyaltyList-SK",  new AttributeValue { S = $"Card#{newLoyaltyCard.Id}" }},
        };
      
        var userRequest = new PutItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Item = userItem,
            ConditionExpression = "attribute_not_exists(PK) AND attribute_not_exists(SK)"
        };
      
        try
        {
            var response = await _dynamoDb.PutItemAsync(userRequest);
        }
        catch (ConditionalCheckFailedException)
        {
            throw new Exception($"Loyalty Card already exists for User {newLoyaltyCard.UserId} and Business {newLoyaltyCard.BusinessId}");
        }

        // TODO: Add error handling based on response
        return newLoyaltyCard;
    }
    
   public Task<IEnumerable<LoyaltyCard>> GetAllAsync() => throw new NotImplementedException();
   // public Task<LoyaltyCard> GetByIdAsync(Guid id) => throw new NotImplementedException();
   public async Task<LoyaltyCard> GetByIdAsync(Guid id, Guid userId)
   {
       var request = new GetItemRequest
       {
           TableName = _dynamoDbSettings.TableName,
           Key = new Dictionary<string, AttributeValue>
           {
               { "PK", new AttributeValue { S = $"User#{userId}" } },
               { "SK", new AttributeValue { S = $"Business#{id}" } }  // this may need adjustment based on your data model
           }
       };

       var response = await _dynamoDb.GetItemAsync(request);

       if (response.Item == null || !response.IsItemSet)
       {
           return null;  // Not found
       }

       return MapToLoyaltyCard(response.Item);
   }

   private LoyaltyCard MapToLoyaltyCard(Dictionary<string, AttributeValue> item)
   {
       return new LoyaltyCard
       {
           Id = Guid.Parse(item["CardId"].S),
           BusinessId = Guid.Parse(item["BusinessId"].S),
           Points = int.Parse(item["StampCount"].N),
           DateIssued = DateTime.Parse(item["DateIssued"].S),
           DateLastStamped = DateTime.Parse(item["LastStampDate"].S),
           Status = (LoyaltyStatus)Enum.Parse(typeof(LoyaltyStatus), item["Status"].S)
       };
   }
   
   public Task UpdateAsync(LoyaltyCard entity) => throw new NotImplementedException();
   public Task DeleteAsync(Guid id) => throw new NotImplementedException();
}