using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Settings;
using Newtonsoft.Json;

namespace LoyaltySystem.Data.Repositories;

public class BusinessRepository : IBusinessRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDbSettings _dynamoDbSettings;

    public BusinessRepository(IAmazonDynamoDB dynamoDb, DynamoDbSettings dynamoDbSettings)
    {
        _dynamoDb         = dynamoDb;
        _dynamoDbSettings = dynamoDbSettings;
    }
    
    public async Task<Business> CreateBusinessAsync(Business newBusiness)
    {
        var openingHoursJson = JsonConvert.SerializeObject(newBusiness.OpeningHours);
        var locationJson     = JsonConvert.SerializeObject(newBusiness.Location);
        
        var item = new Dictionary<string, AttributeValue>
        {
            // New PK and SK patterns
            { "PK",          new AttributeValue { S = "Business#" + newBusiness.Id }},
            { "SK",          new AttributeValue { S = "Meta#BusinessInfo" }},
         
            // New Type attribute
            { "BusinessId",   new AttributeValue { S = newBusiness.Id.ToString()} },
            { "OwnerId",      new AttributeValue { S = newBusiness.OwnerId.ToString()} },
            { "EntityType",   new AttributeValue { S = newBusiness.GetType().Name} },
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

    /*
     public async Task<Campaign> CreateCampaignAsync(Campaign newCampaign)
    {
        
    }
    */
    public async Task UpdatePermissionsAsync(List<Permission> permissions)
    {
        foreach (var permission in permissions)
        {
            var item = new Dictionary<string, AttributeValue>
            {
                // Assuming 'USER#' as a prefix for the user PK and 'BUSINESS#' as a prefix for businesses
                { "PK",         new AttributeValue { S = $"User#{permission.UserId}" }},
                { "SK",         new AttributeValue { S = $"Permission#Business#{permission.BusinessId}" }},
                { "UserId",     new AttributeValue { S = $"{permission.UserId}" }},
                { "BusinessId", new AttributeValue { S = $"{permission.BusinessId}" }},
                { "EntityType", new AttributeValue { S = $"{EntityType.Permission}" }},
                { "Role",       new AttributeValue { S = $"{permission.Role}" }},
                { "Timestamp",  new AttributeValue { S = $"{DateTime.UtcNow}" }},
            
                { "BusinessUserList-PK",  new AttributeValue { S = $"{permission.BusinessId}" }},
                { "BusinessUserList-SK",  new AttributeValue { S = $"Permission#User#{permission.UserId}" }},
            };

            var request = new PutItemRequest
            {
                TableName = _dynamoDbSettings.TableName,
                Item = item,
                ConditionExpression = "attribute_not_exists(PK) AND attribute_not_exists(SK)"
            };

            try
            {
                await _dynamoDb.PutItemAsync(request);
            }
            catch (ConditionalCheckFailedException)
            {
                throw new Exception($"Permissions for Business {permission.BusinessId}, for User {permission.UserId} already exists");
            }
            catch (Exception ex)
            {
                // Handle exception (log it, throw it, etc.)
                throw new Exception($"Failed to update permission for user {permission.UserId} for business {permission.BusinessId}.", ex);
            }
        }
    }
    
   public Task<IEnumerable<Business>> GetAllAsync() => throw new NotImplementedException();
   public Task<Business> GetByIdAsync(Guid id) => throw new NotImplementedException();
   public Task UpdateAsync(Business entity) => throw new NotImplementedException();
   public Task DeleteAsync(Guid id) => throw new NotImplementedException();
}