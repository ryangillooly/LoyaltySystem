using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core;
using LoyaltySystem.Core.DTOs;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Exceptions;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Settings;
using LoyaltySystem.Core.Utilities;
using Newtonsoft.Json;
using static LoyaltySystem.Core.Exceptions.BusinessExceptions;
using static LoyaltySystem.Data.Extensions.DynamoDbExtensions;
using static LoyaltySystem.Core.Models.Constants;

namespace LoyaltySystem.Data.Repositories;

public class BusinessRepository : IBusinessRepository
{
    private readonly IDynamoDbClient _dynamoDbClient;
    private readonly DynamoDbSettings _dynamoDbSettings;

    public BusinessRepository(IDynamoDbClient dynamoDbClient, DynamoDbSettings dynamoDbSettings) =>
        (_dynamoDbClient, _dynamoDbSettings) = (dynamoDbClient, dynamoDbSettings);
    
    // Businesses
    public async Task CreateBusinessAsync(Business newBusiness, BusinessUserPermissions permissions, EmailToken emailToken)
    {
        var permissionItem = new List<BusinessUserPermissions> { permissions }.MapBusinessUserPermissionsToItem()[0];
        var transactWriteItems = new List<TransactWriteItem>
        {
            new ()
            {
                Put = new Put
                {
                    TableName = _dynamoDbSettings.TableName,
                    Item = newBusiness.ToDynamoItem(),
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
            },
            new ()
            {
                Put = new Put
                {
                    TableName = _dynamoDbSettings.TableName,
                    Item = permissionItem
                }
            }
        };
      
        await _dynamoDbClient.TransactWriteItemsAsync(transactWriteItems);
    }
    public async Task<Business> GetBusinessAsync(Guid businessId)
    {
        var request = new GetItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { Pk, new AttributeValue { S = BusinessPrefix + businessId }},
                { Sk, new AttributeValue { S = MetaBusinessInfo  }}
            }
        };

        var response = await _dynamoDbClient.GetItemAsync(request);
        if (response.Item.Count == 0 || !response.IsItemSet) throw new BusinessNotFoundException(businessId);

        return response.Item.MapItemToBusiness();
    }
    public async Task<List<Business>> GetBusinessesAsync(List<Guid> businessIdList)
    {
        var transactItemsList = businessIdList.Select(businessId => ToTransactGetItem(_dynamoDbSettings, BusinessPrefix + businessId, MetaBusinessInfo)).ToList();
        var getBusinessListRequest = new TransactGetItemsRequest { TransactItems = transactItemsList };
        var getItemsResponse = await _dynamoDbClient.TransactGetItemsAsync(getBusinessListRequest);
        return getItemsResponse.Responses.Select(itemResponse => itemResponse.Item).Select(response => response.MapItemToBusiness()).ToList();
    }
    public async Task UpdateBusinessAsync(Business updatedBusiness)
    {
        var dynamoRecord = updatedBusiness.MapBusinessToItem();
        var updateRequest = new UpdateItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {Pk, new AttributeValue { S = dynamoRecord[Pk].S }},
                {Sk, new AttributeValue { S = dynamoRecord[Sk].S }}
            },
            UpdateExpression = @"
            SET  
              #Name        = :name,
              #Status      = :status,
              #Location    = :location, 
              Description  = :desc, 
              Email        = :email, 
              PhoneNumber  = :phoneNumber,
              OpeningHours = :openingHours
         ",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                {":status",       new AttributeValue { S = dynamoRecord[Status].S }},
                {":name",         new AttributeValue { S = dynamoRecord[Name].S }},
                {":openingHours", new AttributeValue { S = dynamoRecord[OpeningHoursAttName].S }},
                {":phoneNumber",  new AttributeValue { S = dynamoRecord[PhoneNumber].S }},
                {":email",        new AttributeValue { S = dynamoRecord[Email].S }},
                {":location",     new AttributeValue { S = dynamoRecord[LocationAttributeName].S }},
                {":desc",         new AttributeValue { S = dynamoRecord[Description].S }}
            },
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                {"#Status",   Status},  // Mapping the alias #St to the actual attribute name "Status"
                {"#Name",     Name},
                {"#Location", LocationAttributeName}
            }
        };
      
        await _dynamoDbClient.UpdateItemAsync(updateRequest);
    }
    // Need to delete all records which are user permissions to the business being deleted
    // Can we delete the record from the GSI and that's it? Or do we need to query the GSI, then perform the delete?
    public async Task DeleteBusinessAsync(Guid businessId)
    {
        var transactWriteItemList = new List<TransactWriteItem>();
        
        // Get all Users who have permissions to the Business
        var getPermissionsRequest = new QueryRequest
        {
            TableName = _dynamoDbSettings.TableName,
            IndexName = _dynamoDbSettings.BusinessUserListGsi,
            KeyConditionExpression = "#PK = :PKValue AND begins_with(#SK, :SKValue)",  // Use placeholders
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#PK", BusinessUserListPk },   // Map to the correct attribute names
                { "#SK", BusinessUserListSk }
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":PKValue", new AttributeValue { S = BusinessPrefix + businessId }},
                { ":SKValue", new AttributeValue { S = PermissionUserPrefix }}
            }
        };
        var getItemResponse = await _dynamoDbClient.QueryAsync(getPermissionsRequest);

        // Loop each user, and add Delete request to TransactWriteItemList
        foreach (var record in getItemResponse.Items)
        {
            var transactWriteItem = new TransactWriteItem
            {
                Delete = new Delete
                {
                    TableName = _dynamoDbSettings.TableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        {Pk, new AttributeValue {S = UserPrefix + record[UserId].S}},
                        {Sk, new AttributeValue {S = PermissionBusinessPrefix + record[BusinessId].S}}
                    }
                }
            };
            
            transactWriteItemList.Add(transactWriteItem);
        }
        
        // Get all Users who have LoyaltyCards with the Business
        var getLoyaltyCardsRequest = new QueryRequest
        {
            TableName = _dynamoDbSettings.TableName,
            IndexName = _dynamoDbSettings.BusinessLoyaltyListGsi,
            KeyConditionExpression = "#PK = :PKValue AND begins_with(#SK, :SKValue)",  // Use placeholders
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#PK", BusinessLoyaltyListPk },   // Map to the correct attribute names
                { "#SK", BusinessLoyaltyListSk }
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":PKValue", new AttributeValue { S = BusinessPrefix + businessId }},
                { ":SKValue", new AttributeValue { S = CardUserPrefix }}
            }
        };
        var getLoyaltyCardsResponse = await _dynamoDbClient.QueryAsync(getLoyaltyCardsRequest);

        // Loop each user, and add Delete request to TransactWriteItemList
        foreach (var record in getLoyaltyCardsResponse.Items)
        {
            var transactWriteItem = new TransactWriteItem
            {
                Delete = new Delete
                {
                    TableName = _dynamoDbSettings.TableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { Pk, new AttributeValue {S = UserPrefix + record[UserId].S}},
                        { Sk, new AttributeValue {S = CardBusinessPrefix + record[BusinessId].S}}
                    }
                }
            };
            
            transactWriteItemList.Add(transactWriteItem);
        }
        
        // Delete all User permissions to the Business being deleted
        if(transactWriteItemList.Count > 0)
            await _dynamoDbClient.TransactWriteItemsAsync(transactWriteItemList);
       
        // Deletes everything that has the PK - Business##<BusinessId>
        await _dynamoDbClient.DeleteItemsWithPkAsync(BusinessPrefix + businessId);
    }
   public async Task VerifyEmailAsync(VerifyBusinessEmailDto dto)
   {
      var getRequest = new GetItemRequest
      {
         TableName = _dynamoDbSettings.TableName,
         Key = new Dictionary<string, AttributeValue>
         {
            { Pk, new AttributeValue { S = BusinessPrefix  + dto.BusinessId }},
            { Sk, new AttributeValue { S = TokenPrefix + dto.Token  }}
         }
      };
      var getResponse  = await _dynamoDbClient.GetItemAsync(getRequest);
      var expiryDate   = DateTime.Parse(getResponse.Item[ExpiryDate].S);
      var status = getResponse.Item[Status].S;
      
      if (getResponse.Item == null)                        throw new EmailExceptions.NoVerificationEmailFoundException(dto.BusinessId, dto.Token);
      if (status == EmailTokenStatus.Verified.ToString())  throw new EmailExceptions.VerificationEmailAlreadyVerifiedException(dto.BusinessId, dto.Token);
      if (DateTime.UtcNow > expiryDate)                    throw new EmailExceptions.VerificationEmailExpiredException(dto.BusinessId, dto.Token);

      var transactWriteList = new List<TransactWriteItem>
      {
         new ()
         {
            Update = new Update
            {
               TableName = _dynamoDbSettings.TableName,
               Key = new Dictionary<string, AttributeValue>
               {
                  { Pk, new AttributeValue { S = BusinessPrefix  + dto.BusinessId }},
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
                  { Pk, new AttributeValue { S = BusinessPrefix + dto.BusinessId }},
                  { Sk, new AttributeValue { S = MetaBusinessInfo }}
               },
               ExpressionAttributeNames = new Dictionary<string, string>
               {
                  { "#Status", Status }
               },
               ExpressionAttributeValues = new Dictionary<string, AttributeValue>
               {
                  {":Status", new AttributeValue { S = $"{BusinessStatus.Active}"}}
               },
               UpdateExpression = "SET #Status = :Status"
            }
         }
      };

      await _dynamoDbClient.TransactWriteItemsAsync(transactWriteList);
   }
    

    // Business User Permissions
    public async Task CreateBusinessUserPermissionsAsync(List<BusinessUserPermissions> newBusinessUserPermissions)
    {
        var dynamoRecords = newBusinessUserPermissions.MapBusinessUserPermissionsToItem();
        await _dynamoDbClient.TransactWriteRecordsAsync(dynamoRecords);
    }
    public async Task UpdateBusinessUserPermissionsAsync(List<BusinessUserPermissions> updatedBusinessUserPermissions)
    {
        var dynamoRecords = updatedBusinessUserPermissions.MapBusinessUserPermissionsToItem();
        var transactWriteItems = dynamoRecords.Select(record => new TransactWriteItem { Put = new Put { TableName = _dynamoDbSettings.TableName, Item = record } }).ToList();

        await _dynamoDbClient.TransactWriteItemsAsync(transactWriteItems);
    }
    public async Task<List<BusinessUserPermissions>> GetBusinessPermissionsAsync(Guid businessId)
    {
        var request = new QueryRequest
        {
            TableName = _dynamoDbSettings.TableName,
            IndexName = _dynamoDbSettings.BusinessUserListGsi,
            KeyConditionExpression = "#PK = :PKValue AND begins_with(#SK, :SKValue)",  // Use placeholders
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#PK", BusinessUserListPk },   // Map to the correct attribute names
                { "#SK", BusinessUserListSk }
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":PKValue", new AttributeValue { S = BusinessPrefix + businessId }},
                { ":SKValue", new AttributeValue { S = PermissionUserPrefix }}
            }
        };

        var response = await _dynamoDbClient.QueryAsync(request);

        if (response.Items is null || response.Count == 0)
            throw new BusinessUsersNotFoundException(businessId);
        
        return response
            .Items
            .Select(permission => 
                new BusinessUserPermissions(
                    Guid.Parse(permission[BusinessId].S), 
                    Guid.Parse(permission[UserId].S), 
                    Enum.Parse<UserRole>(permission[Role].S)))
            .ToList();
    }
    public async Task<BusinessUserPermissions> GetBusinessUsersPermissionsAsync(Guid businessId, Guid userId)
    {
        var request = new GetItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { Pk, new AttributeValue { S = UserPrefix + userId}},
                { Sk, new AttributeValue { S = PermissionBusinessPrefix + businessId }}
            }
        };
        var response = await _dynamoDbClient.GetItemAsync(request);
        if (response.Item is null || !response.IsItemSet) throw new BusinessUserPermissionNotFoundException(userId, businessId);

        return  
            new BusinessUserPermissions 
            (
      Guid.Parse(response.Item[BusinessId].S), 
         Guid.Parse(response.Item[UserId].S), 
           Enum.Parse<UserRole>(response.Item[Role].S)
            );
    }
    public async Task DeleteUsersPermissionsAsync(Guid businessId, List<Guid> userIdList)
    {
        foreach (var userId in userIdList)
        {
            var deleteRequest = new DeleteItemRequest
            {
                TableName = _dynamoDbSettings.TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { Pk, new AttributeValue { S = UserPrefix + userId }},
                    { Sk, new AttributeValue { S = PermissionBusinessPrefix + businessId }}
                }
            };

            await _dynamoDbClient.DeleteItemAsync(deleteRequest); // Replace with batching
        }
    }

    
   // Campaigns
   public async Task CreateCampaignAsync(Campaign newCampaign)
   {
       var dynamoRecord = newCampaign.MapCampaignToItem();
       var putRequest = new PutItemRequest
       {
           TableName = _dynamoDbSettings.TableName,
           Item = dynamoRecord,
           ConditionExpression = "attribute_not_exists(PK) AND attribute_not_exists(SK)"
       };
       await _dynamoDbClient.PutItemAsync(putRequest);
   }
   public async Task<IReadOnlyList<Campaign>?> GetAllCampaignsAsync(Guid businessId)
   {
       var request = new QueryRequest
       {
           TableName = _dynamoDbSettings.TableName,
           KeyConditionExpression = "PK = :businessId AND begins_with(SK, :campaignPrefix)",
           ExpressionAttributeValues = new Dictionary<string, AttributeValue>
           {
               {":businessId",     new AttributeValue { S = BusinessPrefix + businessId }},
               {":campaignPrefix", new AttributeValue { S = CampaignPrefix }}
           }
       };
       var response = await _dynamoDbClient.QueryAsync(request);
       if (response.Items.Count is 0 || response.Items is null) throw new NoCampaignsFoundException(businessId);

       var campaignList = new List<Campaign>();

       foreach (var item in response.Items)
       {
           var campaign     = item.MapItemToCampaign();
           var settings     = new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore };
           var rewards      = JsonConvert.DeserializeObject<List<Reward>>(item[Rewards].S, settings);
           campaign.Rewards = rewards;
           
           campaignList.Add(campaign);
       }

       return campaignList;
   }
   public async Task<Campaign> GetCampaignAsync(Guid businessId, Guid campaignId)
   {
       var request = new GetItemRequest
       {
           TableName = _dynamoDbSettings.TableName,
           Key = new Dictionary<string, AttributeValue>
           {
               { Pk, new AttributeValue { S = BusinessPrefix + businessId }},
               { Sk, new AttributeValue { S = CampaignPrefix + campaignId }}
           }
       };
       var response = await _dynamoDbClient.GetItemAsync(request);
       if (response.Item is null || !response.IsItemSet) throw new CampaignNotFoundException(campaignId, businessId);

       return response.Item.MapItemToCampaign();
   }
   public async Task UpdateCampaignAsync(Campaign updatedCampaign)
   {
       var dynamoRecord = updatedCampaign.MapCampaignToItem();
       await _dynamoDbClient.UpdateRecordAsync(dynamoRecord, null);
   }
   public async Task DeleteCampaignAsync(Guid businessId, List<Guid> campaignIds)
   {
       var batchRequests = new List<WriteRequest>();
       foreach (var campaignId in campaignIds)
       {
           batchRequests.Add(new WriteRequest
           {
               DeleteRequest = new DeleteRequest
               {
                   Key = new Dictionary<string, AttributeValue>
                   {
                       { Pk, new AttributeValue { S = BusinessPrefix + businessId } },
                       { Sk, new AttributeValue { S = CampaignPrefix + campaignId } }
                   }
               }
           });
       }

       // Split requests into chunks of 25, which is the max for a single BatchWriteItem request
       var chunkedBatchRequests = new List<List<WriteRequest>>();
       for (var i = 0; i < batchRequests.Count; i += 25)
       {
           chunkedBatchRequests.Add(batchRequests.GetRange(i, Math.Min(25, batchRequests.Count - i)));
       }

       // Perform the BatchWriteItem for each chunk
       foreach (var chunk in chunkedBatchRequests)
       {
           var batchWriteItemRequest = new BatchWriteItemRequest
           {
               RequestItems = new Dictionary<string, List<WriteRequest>>
               {
                   {_dynamoDbSettings.TableName, chunk}
               }
           };

           try
           {
               await _dynamoDbClient.BatchWriteItemsAsync(batchWriteItemRequest);
           }
           catch (ConditionalCheckFailedException)
           {
               throw new Exception($"Failed to delete items with PK - {BusinessPrefix + businessId} due to condition check");
           }
       }
   }

}