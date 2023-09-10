using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Settings;
using Newtonsoft.Json;

namespace LoyaltySystem.Data.Repositories;

public class BusinessRepository : IRepository<Business>
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDbSettings _dynamoDbSettings;

    public BusinessRepository(IAmazonDynamoDB dynamoDb, DynamoDbSettings dynamoDbSettings)
    {
        _dynamoDb         = dynamoDb;
        _dynamoDbSettings = dynamoDbSettings;
    }
    
    public async Task<Business> AddAsync(Business newBusiness)
    {
        var openingHoursJson = JsonConvert.SerializeObject(newBusiness.OpeningHours);
        var locationJson     = JsonConvert.SerializeObject(newBusiness.Location);
        
        var item = new Dictionary<string, AttributeValue>
        {
            // New PK and SK patterns
            { "PK",          new AttributeValue { S = "BUSINESS#" + newBusiness.Id }},
            { "SK",          new AttributeValue { S = "META#BUSINESSINFO" }},
         
            // New Type attribute
            { "Type",         new AttributeValue { S = newBusiness.GetType().Name} },
            { "Name",         new AttributeValue { S = newBusiness.Name }},
            { "OpeningHours", new AttributeValue { S = openingHoursJson }},
            { "Location",     new AttributeValue { S = locationJson }},
            { "Desc",         new AttributeValue { S = newBusiness.Description }},
            { "PhoneNumber",  new AttributeValue { S = newBusiness.ContactInfo.PhoneNumber }},
            { "Email",        new AttributeValue { S = newBusiness.ContactInfo.Email }},
            { "Status",       new AttributeValue { S = newBusiness.Status.ToString() }},
        };
      
        var request = new PutItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Item = item,
            ConditionExpression = "attribute_not_exists(PK)"
        };
      
        try
        {
            var response = await _dynamoDb.PutItemAsync(request);
        }
        catch (ConditionalCheckFailedException)
        {
            throw new Exception($"Business {newBusiness.Id} is already in use.");
        }

        // TODO: Add error handling based on response
        return newBusiness;
    }
    
   public Task<IEnumerable<Business>> GetAllAsync()
   {
       throw new NotImplementedException();
   }

   public Task<Business> GetByIdAsync(Guid id)
   {
       throw new NotImplementedException();
   }
   
   public Task UpdateAsync(Business entity)
   {
       throw new NotImplementedException();
   }

   public Task DeleteAsync(Guid id)
   {
       throw new NotImplementedException();
   }
}