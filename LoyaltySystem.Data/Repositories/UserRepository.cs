using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Enums;
using static LoyaltySystem.Core.Exceptions.UserExceptions;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Settings;

namespace LoyaltySystem.Data.Repositories;

public class UserRepository : IUserRepository
{
   private readonly IDynamoDbClient _dynamoDbClient;
   private readonly IDynamoDbMapper _dynamoDbMapper;
   private readonly DynamoDbSettings _dynamoDbSettings;
   

   public UserRepository(IDynamoDbClient dynamoDbClient, IDynamoDbMapper dynamoDbMapper, DynamoDbSettings dynamoDbSettings) =>
      (_dynamoDbClient, _dynamoDbMapper, _dynamoDbSettings) = (dynamoDbClient, dynamoDbMapper, dynamoDbSettings);

   public async Task CreateAsync(User newUser)
   {
      var dynamoRecord = _dynamoDbMapper.MapUserToItem(newUser);
      var putRequest = new PutItemRequest
      {
         TableName = _dynamoDbSettings.TableName,
         Item = dynamoRecord,
         ConditionExpression = "attribute_not_exists(PK)"
      };
      await _dynamoDbClient.PutItemAsync(putRequest);
   }
   
   public async Task UpdateUserAsync(User updatedUser)
   {
      var dynamoRecord = _dynamoDbMapper.MapUserToItem(updatedUser);
      await _dynamoDbClient.UpdateRecordAsync(dynamoRecord, null);
   }
   
   public async Task<User?> GetUserAsync(Guid userId)
   {
      var request = new GetItemRequest
      {
         TableName = _dynamoDbSettings.TableName,
         Key = new Dictionary<string, AttributeValue>
         {
            { "PK", new AttributeValue { S = $"User#{userId}" }},
            { "SK", new AttributeValue { S = "Meta#UserInfo"   }}
         }
      };

      var response = await _dynamoDbClient.GetItemAsync(request);

      if (response.Item is null || !response.IsItemSet) 
         throw new UserNotFoundException(userId);
      
      var user = new User
      {
         Id          = Guid.Parse(response.Item["UserId"].S),
         ContactInfo = new ContactInfo
                        {
                           Email       = response.Item["Email"].S, 
                           PhoneNumber = response.Item["PhoneNumber"].S
                        },
         FirstName   = response.Item["FirstName"].S,
         LastName    = response.Item["LastName"].S,
         Status      =  Enum.Parse<UserStatus>(response.Item["Status"].S)
      };

      if (response.Item.ContainsKey("DateOfBirth") && response.Item["DateOfBirth"].S != null)
         user.DateOfBirth = DateTime.Parse(response.Item["DateOfBirth"].S);
      
      return user;
   }
   
   // Not Implemented
   public Task<IEnumerable<User>> GetAllAsync() => throw new NotImplementedException();

   public async Task DeleteUserAsync(Guid userId) => await _dynamoDbClient.DeleteItemsWithPkAsync($"User#{userId}");
}