using Amazon.DynamoDBv2.Model;
using AutoMapper;
using LoyaltySystem.Core.DTOs;
using LoyaltySystem.Core.Enums;
using static LoyaltySystem.Core.Exceptions.EmailExceptions;
using static LoyaltySystem.Core.Exceptions.UserExceptions;
using static LoyaltySystem.Core.Models.Constants;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Models.Persistance;
using LoyaltySystem.Core.Settings;
using LoyaltySystem.Core.Utilities;

namespace LoyaltySystem.Data.Repositories;

public class UserRepository : IUserRepository
{
   private readonly IDynamoDbClient  _dynamoDbClient;
   private readonly DynamoDbSettings _dynamoDbSettings;
   private readonly IMapper          _mapper;
   
   public UserRepository(IDynamoDbClient dynamoDbClient, DynamoDbSettings dynamoDbSettings, IMapper mapper) =>
      (_dynamoDbClient, _dynamoDbSettings, _mapper) = (dynamoDbClient, dynamoDbSettings, mapper);

   public async Task CreateAsync(User newUser, EmailToken emailToken)
   {
      var transactWriteItems = new List<TransactWriteItem>
      {
         CreateUserTransactWriteItem(newUser),
         CreateEmailTokenTransactWriteItem(emailToken)
      };
      
      await _dynamoDbClient.TransactWriteItemsAsync(transactWriteItems);
   }

   private TransactWriteItem CreateEmailTokenTransactWriteItem(EmailToken emailToken) =>
      new ()
      {
         Put = new Put
         {
            TableName = _dynamoDbSettings.TableName,
            Item = emailToken.ToDynamoItem(),
            ConditionExpression = $"attribute_not_exists({Pk}) AND attribute_not_exists({Sk})"
         }
      };
   private TransactWriteItem CreateUserTransactWriteItem(User newUser) =>
      new()
      {
         Put = new Put
         {
            TableName = _dynamoDbSettings.TableName,
            Item = _mapper.Map<UserDynamoModel>(newUser).ToDynamoItem(),
            ConditionExpression = $"attribute_not_exists({Pk})"
         }
      };

   public async Task UpdateUserAsync(User updatedUser)
   {
      var dynamoRecord = _mapper.Map<UserDynamoModel>(updatedUser);
      var updateRequest = new UpdateItemRequest
      {
         TableName = _dynamoDbSettings.TableName,
         Key = new Dictionary<string, AttributeValue>
         {
            { Pk, new AttributeValue { S = dynamoRecord.PK }},
            { Sk, new AttributeValue { S = dynamoRecord.SK }}
         },
         UpdateExpression = @"
            SET  
              FirstName   = :firstName,
              LastName    = :lastName, 
              PhoneNumber = :phoneNumber, 
              Email       = :email, 
              #St         = :status
         ",
         ExpressionAttributeValues = new Dictionary<string, AttributeValue>
         {
            {":status",      new AttributeValue { S = dynamoRecord.Status }},
            {":firstName",   new AttributeValue { S = dynamoRecord.FirstName }},
            {":lastName",    new AttributeValue { S = dynamoRecord.LastName }},
            {":phoneNumber", new AttributeValue { S = dynamoRecord.PhoneNumber }},
            {":email",       new AttributeValue { S = dynamoRecord.Email }}
         },
         ExpressionAttributeNames = new Dictionary<string, string> {{"#St", Status}}
      };
      
      await _dynamoDbClient.UpdateItemAsync(updateRequest);
   }
   public async Task<User?> GetUserAsync(Guid userId)
   {
      var request = new GetItemRequest
      {
         TableName = _dynamoDbSettings.TableName,
         Key = new Dictionary<string, AttributeValue>
         {
            { Pk, new AttributeValue { S = UserPrefix + userId }},
            { Sk, new AttributeValue { S = MetaUserInfo }}
         }
      };

      var response = await _dynamoDbClient.GetItemAsync(request);

      if (response.Item is null || !response.IsItemSet) throw new UserNotFoundException(userId);
      
      return response.Item.FromDynamoItem<User>();
   }
   public Task<IEnumerable<User>> GetAllAsync() => throw new NotImplementedException();
   public async Task DeleteUserAsync(Guid userId) => await _dynamoDbClient.DeleteItemsWithPkAsync(UserPrefix + userId);
   public async Task<List<BusinessUserPermissions>> GetUsersBusinessPermissions(Guid userId)
   {
      var request = new QueryRequest
      {
         TableName = _dynamoDbSettings.TableName,
         KeyConditionExpression = "#PK = :PKValue AND begins_with(#SK, :SKValue)",  // Use placeholders
         ExpressionAttributeNames = new Dictionary<string, string>
         {
            { "#PK", Pk },   // Map to the correct attribute names
            { "#SK", Sk }
         },
         ExpressionAttributeValues = new Dictionary<string, AttributeValue>
         {
            { ":PKValue", new AttributeValue { S = UserPrefix + userId }},
            { ":SKValue", new AttributeValue { S = PermissionBusinessPrefix }}
         }
      };
      
      var response = await _dynamoDbClient.QueryAsync(request);

      var businessUserPermissionsList = new List<BusinessUserPermissions>();
      foreach (var item in response.Items)
      {
         businessUserPermissionsList.Add
         (
            new BusinessUserPermissions
            (
               Guid.Parse(item[BusinessId].S), 
               Guid.Parse(item[UserId].S), 
               Enum.Parse<UserRole>(item[Role].S)
            )
         );
      }

      return businessUserPermissionsList;
   }
   public async Task VerifyEmailAsync(VerifyUserEmailDto dto)
   {
      var getRequest = new GetItemRequest
      {
         TableName = _dynamoDbSettings.TableName,
         Key = new Dictionary<string, AttributeValue>
         {
            { Pk, new AttributeValue { S = UserPrefix  + dto.UserId }},
            { Sk, new AttributeValue { S = TokenPrefix + dto.Token  }}
         }
      };
      var getResponse  = await _dynamoDbClient.GetItemAsync(getRequest);
      var expiryDate   = DateTime.Parse(getResponse.Item[ExpiryDate].S);
      var status = getResponse.Item[Status].S;
      
      if (getResponse.Item == null)                        throw new NoVerificationEmailFoundException(dto.UserId, dto.Token);
      if (status == EmailTokenStatus.Verified.ToString())  throw new VerificationEmailAlreadyVerifiedException(dto.UserId, dto.Token);
      if (DateTime.UtcNow > expiryDate)                    throw new VerificationEmailExpiredException(dto.UserId, dto.Token);

      var transactWriteList = new List<TransactWriteItem>
      {
         new ()
         {
            Update = new Update
            {
               TableName = _dynamoDbSettings.TableName,
               Key = new Dictionary<string, AttributeValue>
               {
                  { Pk, new AttributeValue { S = UserPrefix  + dto.UserId }},
                  { Sk, new AttributeValue { S = TokenPrefix + dto.Token  }}
               },
               ExpressionAttributeNames = new Dictionary<string, string>
               {
                  { "#VerifiedDate", VerifiedDate },
                  { "#Status",       Status }
               },
               ExpressionAttributeValues = new Dictionary<string, AttributeValue>
               {
                  {":VerifiedDate", new AttributeValue { S = $"{DateTime.UtcNow}" }},
                  {":Status",       new AttributeValue { S = $"{EmailTokenStatus.Verified}" }},
               },
               UpdateExpression = "SET #Status = :Status, #VerifiedDate = :VerifiedDate"
            }
         },
         new ()
         {
            Update = new Update
            {
               TableName = _dynamoDbSettings.TableName,
               Key = new Dictionary<string, AttributeValue>
               {
                  { Pk, new AttributeValue { S = UserPrefix + dto.UserId }},
                  { Sk, new AttributeValue { S = MetaUserInfo }}
               },
               ExpressionAttributeNames = new Dictionary<string, string>
               {
                  { "#Status", Status }
               },
               ExpressionAttributeValues = new Dictionary<string, AttributeValue>
               {
                  {":Status", new AttributeValue { S = $"{UserStatus.Active}"}}
               },
               UpdateExpression = "SET #Status = :Status"
            }
         }
      };

      await _dynamoDbClient.TransactWriteItemsAsync(transactWriteList);
   }
}