using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.DTOs;
using LoyaltySystem.Core.Enums;
using static LoyaltySystem.Core.Exceptions.EmailExceptions;
using static LoyaltySystem.Core.Exceptions.UserExceptions;
using static LoyaltySystem.Core.Models.Constants;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Settings;
using LoyaltySystem.Core.Utilities;
using Newtonsoft.Json;

namespace LoyaltySystem.Data.Repositories;

public class UserRepository : IUserRepository
{
   private readonly IDynamoDbClient _dynamoDbClient;
   private readonly DynamoDbSettings _dynamoDbSettings;
   private readonly IEmailService _emailService;
   private readonly EmailSettings _emailSettings;
   

   public UserRepository(IDynamoDbClient dynamoDbClient, DynamoDbSettings dynamoDbSettings, IEmailService emailService, EmailSettings emailSettings) =>
      (_dynamoDbClient, _dynamoDbSettings, _emailService, _emailSettings) = (dynamoDbClient, dynamoDbSettings, emailService, emailSettings);

   public async Task CreateAsync(User newUser, EmailToken emailToken)
   {
      var transactWriteItems = new List<TransactWriteItem>
      {
         new ()
         {
            Put = new Put
            {
               TableName = _dynamoDbSettings.TableName,
               Item = newUser.ToDynamoItem(),
               ConditionExpression = "attribute_not_exists(PK)"
            }
         },
         new ()
         {
            Put = new Put
            {
               TableName = _dynamoDbSettings.TableName,
               Item = emailToken.ToDynamoItem(),
               ConditionExpression = "attribute_not_exists(PK) AND attribute_not_exists(SK)"
            }
         }
      };
      
      await _dynamoDbClient.TransactWriteItemsAsync(transactWriteItems);
   }

   public async Task SendVerificationEmailAsync(string email, Guid userId, Guid token)
   {
      var verificationLink = $"http://localhost:3000/user/{userId}/verify-email/{token}";
      var emailInfo = new EmailInfo
      {
         ToEmail   = email,
         FromEmail = _emailSettings.From,
         Subject   = "Loyalty System - Verification",
         Body      = $"Please verify your account by going to the following URL - {verificationLink}"
      };
      await _emailService.SendEmail(emailInfo);
   }
   
   public async Task UpdateUserAsync(User updatedUser)
   {
      var dynamoRecord = updatedUser.MapUserToItem();
      var updateRequest = new UpdateItemRequest
      {
         TableName = _dynamoDbSettings.TableName,
         Key = new Dictionary<string, AttributeValue>
         {
            {"PK", new AttributeValue { S = dynamoRecord["PK"].S }},
            {"SK", new AttributeValue { S = dynamoRecord["SK"].S }}
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
            {":status",      new AttributeValue { S = dynamoRecord["Status"].S }},
            {":firstName",   new AttributeValue { S = dynamoRecord["FirstName"].S }},
            {":lastName",    new AttributeValue { S = dynamoRecord["LastName"].S }},
            {":phoneNumber", new AttributeValue { S = dynamoRecord["PhoneNumber"].S }},
            {":email",       new AttributeValue { S = dynamoRecord["Email"].S }}
         },
         ExpressionAttributeNames = new Dictionary<string, string>
         {
            {"#St", "Status"}  // Mapping the alias #St to the actual attribute name "Status"
         }
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
      
      return response.FromDynamoItem<User>();
   }

   public Task<IEnumerable<User>> GetAllAsync() => throw new NotImplementedException();

   public async Task DeleteUserAsync(Guid userId) => await _dynamoDbClient.DeleteItemsWithPkAsync($"User#{userId}");

   public async Task<List<BusinessUserPermissions>> GetUsersBusinessPermissions(Guid userId)
   {
      var request = new QueryRequest
      {
         TableName = _dynamoDbSettings.TableName,
         KeyConditionExpression = "#PK = :PKValue AND begins_with(#SK, :SKValue)",  // Use placeholders
         ExpressionAttributeNames = new Dictionary<string, string>
         {
            { "#PK", "PK" },   // Map to the correct attribute names
            { "#SK", "SK" }
         },
         ExpressionAttributeValues = new Dictionary<string, AttributeValue>
         {
            { ":PKValue", new AttributeValue { S = $"User#{userId}" }},
            { ":SKValue", new AttributeValue { S = "Permission#Business" }}
         }
      };
      
      var response = await _dynamoDbClient.QueryAsync(request);

      var businessUserPermissionsList = new List<BusinessUserPermissions>();
      foreach (var item in response.Items)
      {
         businessUserPermissionsList.Add(new BusinessUserPermissions(Guid.Parse(item["BusinessId"].S), Guid.Parse(item["UserId"].S), Enum.Parse<UserRole>(item["Role"].S)));
      }

      return businessUserPermissionsList;
   }

   public async Task VerifyEmailAsync(VerifyEmailDto dto)
   {
      var getRequest = new GetItemRequest
      {
         TableName = _dynamoDbSettings.TableName,
         Key = new Dictionary<string, AttributeValue>
         {
            {"PK", new AttributeValue {S = $"User#{dto.UserId}" }},
            {"SK", new AttributeValue {S = $"Token#{dto.Token}" }}
         }
      };
      var getResponse  = await _dynamoDbClient.GetItemAsync(getRequest);
      var expiryDate   = DateTime.Parse(getResponse.Item["ExpiryDate"].S);
      var status = getResponse.Item["Status"].S;
      
      if (getResponse.Item == null)     throw new NoVerificationEmailFoundException(dto.UserId, dto.Token);
      if (status == "Verified")         throw new VerificationEmailAlreadyVerifiedException(dto.UserId, dto.Token);
      if (DateTime.UtcNow > expiryDate) throw new VerificationEmailExpiredException(dto.UserId, dto.Token);

      var transactWriteList = new List<TransactWriteItem>
      {
         new ()
         {
            Update = new Update
            {
               TableName = _dynamoDbSettings.TableName,
               Key = new Dictionary<string, AttributeValue>
               {
                  { "PK", new AttributeValue { S = $"User#{dto.UserId}" } },
                  { "SK", new AttributeValue { S = $"Token#{dto.Token}" } }
               },
               ExpressionAttributeNames = new Dictionary<string, string>
               {
                  { "#VerifiedDate", "VerifiedDate" },
                  { "#Status",       "Status" }
               },
               ExpressionAttributeValues = new Dictionary<string, AttributeValue>
               {
                  {":VerifiedDate", new AttributeValue { S = $"{DateTime.UtcNow}" }},
                  {":Status",       new AttributeValue { S = "Verified" }},
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
                  { "PK", new AttributeValue { S = $"User#{dto.UserId}" }},
                  { "SK", new AttributeValue { S = "Meta#UserInfo"      }}
               },
               ExpressionAttributeNames = new Dictionary<string, string>
               {
                  { "#Status", "Status" }
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