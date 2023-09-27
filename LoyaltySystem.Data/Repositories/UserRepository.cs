using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Data.Clients;
namespace LoyaltySystem.Data.Repositories;

public class UserRepository : IUserRepository
{
   private readonly IDynamoDbClient _dynamoDbClient;
   private readonly IDynamoDbMapper _dynamoDbMapper ;
   

   public UserRepository(IDynamoDbClient dynamoDbClient, IDynamoDbMapper dynamoDbMapper) =>
      (_dynamoDbClient, _dynamoDbMapper) = (dynamoDbClient, dynamoDbMapper);

   public async Task CreateAsync(User newUser)
   {
      var dynamoRecord = _dynamoDbMapper.MapUserToItem(newUser);
      await _dynamoDbClient.WriteRecord(dynamoRecord, "attribute_not_exists(PK)");
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