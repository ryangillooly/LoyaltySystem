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
      await _dynamoDbClient.WriteRecordAsync(dynamoRecord, "attribute_not_exists(PK)");
   }
   
   public async Task UpdateUserAsync(User updatedUser)
   {
      var dynamoRecord = _dynamoDbMapper.MapUserToItem(updatedUser);
      await _dynamoDbClient.UpdateRecordAsync(dynamoRecord, null);
   }
   
   public async Task<User> GetByIdAsync(Guid id)
   {
      var response = await _dynamoDbClient.GetUserByIdAsync(id);
      var user = new User
      {
         Id = Guid.Parse(response.Item["UserId"].S),
         ContactInfo = new ContactInfo
                        {
                           Email       = response.Item["Email"].S, 
                           PhoneNumber = response.Item["PhoneNumber"].S
                        },
         FirstName = response.Item["FirstName"].S,
         LastName = response.Item["LastName"].S,
         //Permissions = response.Item["Permissions"].S, // Deserialize Json to List<UserPermission>
         Status =  Enum.Parse<UserStatus>(response.Item["Status"].S)
      };

      if (response.Item.ContainsKey("DateOfBirth") && response.Item["DateOfBirth"].S != null)
         user.DateOfBirth = DateTime.Parse(response.Item["DateOfBirth"].S);

      
      return user;
   }
   
   // Not Implemented
   public Task<IEnumerable<User>> GetAllAsync() => throw new NotImplementedException();

   public async Task DeleteUserAsync(Guid userId) => await _dynamoDbClient.DeleteUserAsync(userId);
}