using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Extensions;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace LoyaltySystem.Data.Repositories;

public class UserRepository : IUserRepository
{
   private readonly IAmazonDynamoDB _dynamoDb;
   private readonly DynamoDbSettings _dynamoDbSettings;
   private readonly IAuditService _auditService;
   
   public UserRepository(IAmazonDynamoDB dynamoDb, DynamoDbSettings dynamoDbSettings, IAuditService auditService)
   {
      _dynamoDb         = dynamoDb;
      _dynamoDbSettings = dynamoDbSettings;
      _auditService     = auditService;
   }

   public async Task CreateUserAsync(User newUser)
   {
      var item = new Dictionary<string, AttributeValue>
      {
         // New PK and SK patterns
         { "PK", new AttributeValue { S = "User#" + newUser.Id } },
         { "SK", new AttributeValue { S = "Meta#UserInfo" } },

         // New Type attribute
         { "UserId", new AttributeValue { S = newUser.Id.ToString() } },
         { "EntityType", new AttributeValue { S = newUser.GetType().Name.ToPascalCase() } },
         { "Email", new AttributeValue { S = newUser.ContactInfo.Email } },
         { "PhoneNumber", new AttributeValue { S = newUser.ContactInfo.PhoneNumber } },
         { "FirstName", new AttributeValue { S = newUser.FirstName.ToPascalCase() } },
         { "LastName", new AttributeValue { S = newUser.LastName.ToPascalCase() } },
         { "Status", new AttributeValue { S = newUser.Status.ToString().ToPascalCase() } },
      };

      // Conditionally add DateOfBirth if it's not null
      if (newUser.DateOfBirth.HasValue)
         item["DateOfBirth"] = new AttributeValue { S = newUser.DateOfBirth.Value.ToString("yyyy-MM-dd") };

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
         throw new Exception($"Id {newUser.Id} is already in use.");
      }
      
      
   }
   public async Task<bool> DoesEmailExistAsync(string email)
   {
      var request = new QueryRequest
      {
         TableName = _dynamoDbSettings.TableName,
         IndexName = "Emails", // Use GSI for querying
         KeyConditionExpression = "Email = :emailValue", // Assuming your GSI PK is named "Email"
         ExpressionAttributeValues = new Dictionary<string, AttributeValue>
         {
            {":emailValue", new AttributeValue { S = email } }
         },
         Limit = 1 // We only need to know if at least one item exists
      };

      var response = await _dynamoDb.QueryAsync(request);
      return response.Count > 0; // If count > 0, email exists
   }
   public async Task<User> GetByIdAsync(Guid id)
   {
      var request = new GetItemRequest
      {
         TableName = _dynamoDbSettings.TableName,
         Key = new Dictionary<string, AttributeValue> { { "UserId", new AttributeValue { S = id.ToString() } } }
      };

      var response = await _dynamoDb.GetItemAsync(request);

      if (response.Item == null || !response.IsItemSet)
      {
         return null;  // or handle accordingly
      }

      var user = new User
      {
         //Id = Guid.Parse(response.Item["UserId"].S),
         ContactInfo = new ContactInfo
                        {
                           Email       = response.Item["Email"].S, 
                           PhoneNumber = response.Item["PhoneNumber"].S
                        },
         FirstName = response.Item["FirstName"].S,
         LastName = response.Item["LastName"].S,
         DateOfBirth = DateTime.Parse(response.Item["DateOfBirth"].S),
         //Permissions = response.Item["Permissions"].S, // Deserialize Json to List<UserPermission>
         Status =  Enum.Parse<UserStatus>(response.Item["Status"].S)
      };

      return user;
   }

   public async Task UpdatePermissionsAsync(List<UserPermission> permissions)
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
            Item = item
         };

         try
         {
            await _dynamoDb.PutItemAsync(request);
         }
         catch (Exception ex)
         {
            // Handle exception (log it, throw it, etc.)
            throw new Exception($"Failed to update permission for user {permission.UserId} for business {permission.BusinessId}.", ex);
         }
      }
   }

   // Not Implemented
   public Task<IEnumerable<User>> GetAllAsync() => throw new NotImplementedException();
   public Task DeleteAsync(Guid id) => throw new NotImplementedException();

   public Task UpdateAsync(User entity) => throw new NotImplementedException();
}