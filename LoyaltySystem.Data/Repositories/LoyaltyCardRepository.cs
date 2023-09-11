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
    
    public async Task<LoyaltyCard> AddAsync(LoyaltyCard newLoyaltyCard)
    {
        
        var businessItem = new Dictionary<string, AttributeValue>
        {
            // New PK and SK patterns
            { "PK",          new AttributeValue { S = "BUSINESS#" + newLoyaltyCard.BusinessId }},
            { "SK",          new AttributeValue { S = "CARD#"     + newLoyaltyCard.Id }},
            
            // New Type attribute
            { "Id",           new AttributeValue { S = newLoyaltyCard.Id.ToString() }},
            { "BusinessId",   new AttributeValue { S = newLoyaltyCard.BusinessId.ToString() }},
            { "Type",         new AttributeValue { S = newLoyaltyCard.GetType().Name }},
        };
      
        var businessRequest = new PutItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Item = businessItem,
            ConditionExpression = "attribute_not_exists(PK) AND attribute_not_exists(SK)"
        };
      
        try
        {
            var response = await _dynamoDb.PutItemAsync(businessRequest);
        }
        catch (ConditionalCheckFailedException)
        {
            throw new Exception($"Cannot create Business Loyalty Card");
        }
        
        //************************************************
        
        var userItem = new Dictionary<string, AttributeValue>
        {
            // New PK and SK patterns
            { "PK",          new AttributeValue { S = "USER#"     + newLoyaltyCard.UserEmail }},
            { "SK",          new AttributeValue { S = "BUSINESS#" + newLoyaltyCard.BusinessId }},
            
            // New Type attribute
            { "CardId",        new AttributeValue { S = newLoyaltyCard.Id.ToString() }},
            { "BusinessId",    new AttributeValue { S = newLoyaltyCard.BusinessId.ToString() }},
            { "Type",          new AttributeValue { S = newLoyaltyCard.GetType().Name }},
            { "StampCount",    new AttributeValue { N = newLoyaltyCard.StampCount.ToString() }},
            { "DateIssued",    new AttributeValue { S = newLoyaltyCard.DateIssued.ToString() }},
            { "LastStampDate", new AttributeValue { S = newLoyaltyCard.DateLastStamped.ToString() }},
            { "Status",        new AttributeValue { S = newLoyaltyCard.Status.ToString() }},
        };
      
        var userRequest = new PutItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Item = userItem
        };
      
        try
        {
            var response = await _dynamoDb.PutItemAsync(userRequest);
        }
        catch (ConditionalCheckFailedException)
        {
            throw new Exception($"Cannot create user loyalty card");
        }

        // TODO: Add error handling based on response
        return newLoyaltyCard;
    }
    
   public Task<IEnumerable<LoyaltyCard>> GetAllAsync() => throw new NotImplementedException();
   // public Task<LoyaltyCard> GetByIdAsync(Guid id) => throw new NotImplementedException();
   public async Task<LoyaltyCard> GetByIdAsync(Guid id, string userEmail)
   {
       var request = new GetItemRequest
       {
           TableName = _dynamoDbSettings.TableName,
           Key = new Dictionary<string, AttributeValue>
           {
               { "PK", new AttributeValue { S = $"USER#{userEmail}" } },
               { "SK", new AttributeValue { S = $"BUSINESS#{id}" } }  // this may need adjustment based on your data model
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
           StampCount = int.Parse(item["StampCount"].N),
           DateIssued = DateTime.Parse(item["DateIssued"].S),
           DateLastStamped = DateTime.Parse(item["LastStampDate"].S),
           Status = (LoyaltyStatus)Enum.Parse(typeof(LoyaltyStatus), item["Status"].S)
       };
   }
   
   public Task UpdateAsync(LoyaltyCard entity) => throw new NotImplementedException();
   public Task DeleteAsync(Guid id) => throw new NotImplementedException();
}