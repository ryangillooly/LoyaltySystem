using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface IDynamoDbClient
{
    // Users
    Task<GetItemResponse?> GetUserAsync(Guid userId);
    
    // Business
    Task<GetItemResponse?> GetBusinessAsync(Guid businessId);
    
    // Business Campaigns
    Task<GetItemResponse?> GetCampaignAsync(Guid businessId, Guid campaignId);
    Task<QueryResponse?> GetAllCampaignsAsync(Guid businessId);
    Task DeleteCampaignAsync(Guid businessId, List<Guid> campaignIds);
    
    // Business Users
    Task<QueryResponse?> GetBusinessPermissions(Guid businessId);
    Task<GetItemResponse?> GetBusinessUsersPermissions(Guid businessId, Guid userId);
    Task DeleteBusinessUsersPermissions(Guid businessId, List<Guid> userId);
    

    // General
    Task UpdateRecordAsync(Dictionary<string, AttributeValue> item, string? conditionExpression);
    Task DeleteItemsWithPkAsync(string pk);
    Task TransactWriteItemsAsync(List<TransactWriteItem> transactWriteItems);
    Task BatchWriteRecordsAsync(List<Dictionary<string, AttributeValue>> items);
    Task TransactWriteRecordsAsync(List<Dictionary<string, AttributeValue>> items);
    
    // Dynamo Methods
    Task<PutItemResponse> PutItemAsync(PutItemRequest request);
    Task<DeleteItemResponse> DeleteItemAsync(DeleteItemRequest request);
    Task<UpdateItemResponse> UpdateItemAsync(UpdateItemRequest request);
    Task<GetItemResponse> GetItemAsync(GetItemRequest request);
    Task<QueryResponse> QueryAsync(QueryRequest request);
    Task<ScanResponse> ScanAsync(ScanRequest request);
    Task<BatchWriteItemResponse> BatchWriteItemsAsync(BatchWriteItemRequest request);

}