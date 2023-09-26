using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Utilities;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Settings;
using LoyaltySystem.Data.Clients;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace LoyaltySystem.Data.Repositories;

public class UserRepository : IUserRepository
{
   private readonly IDynamoDbClient _dynamoDbClient;
   private readonly IAuditService _auditService;

   public UserRepository(IDynamoDbClient dynamoDbClient, IAuditService auditService) =>
      (_dynamoDbClient, _auditService) = (dynamoDbClient, auditService);

   public async Task CreateAsync(User newUser)
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

      await _dynamoDbClient.WriteRecord(item, "attribute_not_exists(PK)");
   }
   
   public async Task<User> GetByIdAsync(Guid id)
   {
      var response = await _dynamoDbClient.GetUserById(id);
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
   
   // Not Implemented
   public Task<IEnumerable<User>> GetAllAsync() => throw new NotImplementedException();
   public Task DeleteAsync(Guid id) => throw new NotImplementedException();

   public Task UpdateAsync(User entity) => throw new NotImplementedException();
}