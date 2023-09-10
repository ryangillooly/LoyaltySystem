using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Enums;
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

   public UserRepository(IAmazonDynamoDB dynamoDb, DynamoDbSettings dynamoDbSettings)
   {
      _dynamoDb         = dynamoDb;
      _dynamoDbSettings = dynamoDbSettings;
   }

   public async Task<User> AddAsync(User newUser)
   {
      // Serializing the Permissions dictionary
      var serializedPermissions = JsonConvert.SerializeObject(newUser.Permissions);
      
      var item = new Dictionary<string, AttributeValue>
      {
         // New PK and SK patterns
         { "PK",          new AttributeValue { S = "USER#" + newUser.ContactInfo.Email.ToLower() } },
         { "SK",          new AttributeValue { S = "META#USERINFO" } },
         
         // New Type attribute
         { "Type",        new AttributeValue { S = newUser.GetType().Name} },
         { "Email",       new AttributeValue { S = newUser.ContactInfo.Email } },
         { "PhoneNumber", new AttributeValue { S = newUser.ContactInfo.PhoneNumber } },
         { "FirstName",   new AttributeValue { S = newUser.FirstName } },
         { "LastName",    new AttributeValue { S = newUser.LastName } },
         { "Status",      new AttributeValue { S = newUser.Status.ToString() } },
         
         // Serialized Permissions attribute
         { "UserPermissions", new AttributeValue { S = serializedPermissions } }
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
         throw new Exception($"Email {newUser.ContactInfo.Email} is already in use.");
      }

      // TODO: Add error handling based on response
      return newUser;
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

   public async Task<bool> ValidateAsync(UserLoginDTO userLoginDto)
   {
      // Make call to DynamoDb to validate Email + Pass
      var request = new GetItemRequest
      {
         TableName = _dynamoDbSettings.TableName,
         Key = new Dictionary<string, AttributeValue> { { "Email", new AttributeValue { S = userLoginDto.Email } } }
      };

      var response = await _dynamoDb.GetItemAsync(request);

      return response.Item != null && response.IsItemSet;
   }
   
   
   // Not Implemented
   public Task<IEnumerable<User>> GetAllAsync() => throw new NotImplementedException();
   public Task DeleteAsync(Guid id) => throw new NotImplementedException();

   public Task UpdateAsync(User entity) => throw new NotImplementedException();
}