using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.DTOs;
using LoyaltySystem.Core.Enums;
using static LoyaltySystem.Core.Exceptions.UserExceptions;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Settings;
using Newtonsoft.Json;

namespace LoyaltySystem.Data.Repositories;

public class UserRepository : IUserRepository
{
   private readonly IDynamoDbClient _dynamoDbClient;
   private readonly IDynamoDbMapper _dynamoDbMapper;
   private readonly DynamoDbSettings _dynamoDbSettings;
   private readonly IEmailService _emailService;
   private readonly EmailSettings _emailSettings;
   

   public UserRepository(IDynamoDbClient dynamoDbClient, IDynamoDbMapper dynamoDbMapper, DynamoDbSettings dynamoDbSettings, IEmailService emailService, EmailSettings emailSettings) =>
      (_dynamoDbClient, _dynamoDbMapper, _dynamoDbSettings, _emailService, _emailSettings) = (dynamoDbClient, dynamoDbMapper, dynamoDbSettings, emailService, emailSettings);

   public async Task CreateAsync(User newUser, Guid token)
   {
      var dynamoRecord = _dynamoDbMapper.MapUserToItem(newUser);
      var transactWriteItems = new List<TransactWriteItem>
      {
         new ()
         {
            Put = new Put
            {
               TableName = _dynamoDbSettings.TableName,
               Item = dynamoRecord,
               ConditionExpression = "attribute_not_exists(PK)"
            }
         },
         new ()
         {
            Put = new Put
            {
               TableName = _dynamoDbSettings.TableName,
               Item = new Dictionary<string, AttributeValue>
               {
                  { "PK",         new AttributeValue {S = $"User#{newUser.Id}" }},
                  { "SK",         new AttributeValue {S = $"Token#{token}"}},
                  { "EntityType", new AttributeValue {S = "Email Token"}},
                  { "Status",     new AttributeValue {S = "Unverified"}},
                  { "ExpiryDate", new AttributeValue {S = $"{DateTime.UtcNow.AddHours(24)}"}}
               },
               ConditionExpression = "attribute_not_exists(PK) AND attribute_not_exists(SK)"
            }
         }
      };

      
      await _dynamoDbClient.TransactWriteItemsAsync(transactWriteItems);
   }

   public async Task SendVerificationEmailAsync(string email, Guid token)
   {
      var verificationLink = $"http://localhost:3000/verify-email/{token}";
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
      var dynamoRecord = _dynamoDbMapper.MapUserToItem(updatedUser);
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
         Status      = Enum.Parse<UserStatus>(response.Item["Status"].S)
      };

      if (response.Item.ContainsKey("DateOfBirth") && response.Item["DateOfBirth"].S != null)
         user.DateOfBirth = DateTime.Parse(response.Item["DateOfBirth"].S);
      
      return user;
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

      var response = await _dynamoDbClient.GetItemAsync(getRequest);
      var expiryDate = DateTime.Parse(response.Item["ExpiryDate"].S);
      if(DateTime.UtcNow < expiryDate) throw new 
   }
}