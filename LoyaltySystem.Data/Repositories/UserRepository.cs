using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Data.Repositories;

public class UserRepository : IRepository<User>
{
   private readonly IAmazonDynamoDB _dynamoDb;
   private const string TableName = "Users";

   public UserRepository(IAmazonDynamoDB dynamoDb) => _dynamoDb = dynamoDb;
    
   public Task<User> AddAsync(User newUser)
   {
      var request = new PutItemRequest
      {
          TableName = TableName,
          Item = new Dictionary<string, AttributeValue>
          {
              { "UserId", new AttributeValue { S = Guid.NewGuid().ToString() } },
              { "Username", new AttributeValue { S = newUser.Username } },
              { "Email", new AttributeValue { S = newUser.ContactInfo.Email } },
              { "PhoneNumber", new AttributeValue { S = newUser.ContactInfo.PhoneNumber } },
              { "PasswordHash", new AttributeValue { S = newUser.PasswordHash } },
              { "FirstName", new AttributeValue { S = newUser.FirstName } },
              { "LastName", new AttributeValue { S = newUser.LastName } },
              { "DateOfBirth", new AttributeValue { S = newUser.DateOfBirth.ToString("yyyy-MM-dd")} },
              { "Role", new AttributeValue { S = newUser.Role.ToString() } },
              { "Status", new AttributeValue { S = newUser.Status.ToString() } },
              // Map other properties
          }
      };

      var response = _dynamoDb.PutItemAsync(request).Result;

      // TODO: Add error handling based on response
      return Task.FromResult(newUser);
   }

   public Task<IEnumerable<User>> GetAllAsync()
    {
       throw new NotImplementedException();
    }
   public async Task<User> GetByIdAsync(Guid id)
   {
      var request = new GetItemRequest
      {
         TableName = TableName,
         Key = new Dictionary<string, AttributeValue> { { "UserId", new AttributeValue { S = id.ToString() } } }
      };

      var response = await _dynamoDb.GetItemAsync(request);

      if (response.Item == null || !response.IsItemSet)
      {
         return null;  // or handle accordingly
      }

      var user = new User
      {
         // Assuming User class has these properties. Map the DynamoDB AttributeValue to your User properties.
         Id = Guid.Parse(response.Item["UserId"].S),
         Username = response.Item["Username"].S,
         ContactInfo = new ContactInfo
                        {
                           Email       = response.Item["Email"].S, 
                           PhoneNumber = response.Item["PhoneNumber"].S
                        },
         PasswordHash = response.Item["PasswordHash"].S,
         FirstName = response.Item["FirstName"].S,
         LastName = response.Item["LastName"].S,
         DateOfBirth = DateTime.Parse(response.Item["DateOfBirth"].S),
         Role = Enum.Parse<UserRole>(response.Item["Role"].S),
         Status =  Enum.Parse<UserStatus>(response.Item["Status"].S)
      };

      return user;
   }

   public Task UpdateAsync(User entity)
   {
      throw new NotImplementedException();
   }
   public Task DeleteAsync(Guid id)
   {
      throw new NotImplementedException();
   }
}