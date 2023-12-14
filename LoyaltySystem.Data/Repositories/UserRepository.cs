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
   public async Task UpdateUserAsync(User updatedUser)
   {
      var dynamoRecord = _mapper.Map<UserDynamoModel>(updatedUser);
      var updateRequest = UpdateUserItemRequest(dynamoRecord);
      
      await _dynamoDbClient.UpdateItemAsync(updateRequest);
   }
   public async Task DeleteUserAsync(Guid userId) => await _dynamoDbClient.DeleteItemsWithPkAsync(UserPrefix + userId);
   public async Task<User?> GetUserAsync(Guid userId)
   {
      var request  = GetUsersGetItemRequest(userId);
      var response = await _dynamoDbClient.GetItemAsync(request);

      if (response.Item is null || !response.IsItemSet) throw new UserNotFoundException(userId);
      
      var item = response.Item.FromDynamoItem<UserDynamoModel>();
      return _mapper.Map<User>(item);
   }
   public async Task<List<BusinessUserPermissions>> GetUsersBusinessPermissions(Guid userId)
   {
      var request  = GetUsersBusinessPermissionsQueryRequest(userId);
      var response = await _dynamoDbClient.QueryAsync(request);

      return response.Items
         .Select(item       => item.FromDynamoItem<BusinessUserPermissionsDynamoModel>())
         .Select(dynamoItem => _mapper.Map<BusinessUserPermissions>(dynamoItem))
         .ToList();
   }
   public Task<IEnumerable<User>> GetAllAsync() => throw new NotImplementedException();
   
   
   // HELPERS
   public async Task VerifyEmailAsync(VerifyUserEmailDto dto)
   {
      var getRequest = GetEmailTokenItemRequest(dto);
      
      await ValidateEmailAsync(dto, getRequest);

      var transactWriteList = new List<TransactWriteItem>
      {
         UpdateEmailTokenTransactWriteItem(dto),
         UpdateUserTransactWriteItem(dto)
      };

      await _dynamoDbClient.TransactWriteItemsAsync(transactWriteList);
   }
   private async Task ValidateEmailAsync(VerifyUserEmailDto dto, GetItemRequest getRequest)
   {
      var response = await _dynamoDbClient.GetItemAsync(getRequest);
      if (response.Item == null) throw new NoVerificationEmailFoundException(dto.UserId, dto.Token);

      var expiryDate = DateTime.Parse(response.Item[ExpiryDate].S);
      var status = response.Item[Status].S;

      if (status == EmailTokenStatus.Verified.ToString())
         throw new VerificationEmailAlreadyVerifiedException(dto.UserId, dto.Token);
      if (DateTime.UtcNow > expiryDate) throw new VerificationEmailExpiredException(dto.UserId, dto.Token);
   }
   private GetItemRequest GetUsersGetItemRequest(Guid userId)
   {
      return new GetItemRequest
      {
         TableName = _dynamoDbSettings.TableName,
         Key = new Dictionary<string, AttributeValue>
         {
            { Pk, new AttributeValue { S = UserPrefix + userId }},
            { Sk, new AttributeValue { S = MetaUserInfo }}
         }
      };
   }
   private GetItemRequest GetEmailTokenItemRequest(VerifyUserEmailDto dto)
   {
      return new GetItemRequest
      {
         TableName = _dynamoDbSettings.TableName,
         Key = new Dictionary<string, AttributeValue>
         {
            { Pk, new AttributeValue { S = UserPrefix  + dto.UserId }},
            { Sk, new AttributeValue { S = TokenPrefix + dto.Token  }}
         }
      };
   }
   private QueryRequest GetUsersBusinessPermissionsQueryRequest(Guid userId)
   {
      return new QueryRequest
      {
         TableName = _dynamoDbSettings.TableName,
         KeyConditionExpression = "#PK = :PKValue AND begins_with(#SK, :SKValue)",
         ExpressionAttributeNames = new Dictionary<string, string>
         {
            { "#PK", Pk }, 
            { "#SK", Sk }
         },
         ExpressionAttributeValues = new Dictionary<string, AttributeValue>
         {
            { ":PKValue", new AttributeValue { S = UserPrefix + userId }},
            { ":SKValue", new AttributeValue { S = PermissionBusinessPrefix }}
         }
      };
   }
   private UpdateItemRequest UpdateUserItemRequest(UserDynamoModel dynamoRecord)
   {
      return new UpdateItemRequest
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
   }

   private TransactWriteItem CreateEmailTokenTransactWriteItem(EmailToken emailToken)
   {
      var emailTokenDynamoModel = _mapper.Map<UserEmailTokenDynamoModel>(emailToken);
      return new TransactWriteItem
      {
         Put = new Put
         {
            TableName = _dynamoDbSettings.TableName,
            Item = emailTokenDynamoModel.ToDynamoItem(),
            ConditionExpression = $"attribute_not_exists({Pk}) AND attribute_not_exists({Sk})"
         }
      };
   }

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
   private TransactWriteItem UpdateEmailTokenTransactWriteItem(VerifyUserEmailDto dto)
   {
      return new ()
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
      };
   }
   private TransactWriteItem UpdateUserTransactWriteItem(VerifyUserEmailDto dto)
   {
      return new ()
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
      };
   }
}