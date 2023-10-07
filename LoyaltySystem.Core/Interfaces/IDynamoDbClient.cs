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
    
    // LoyaltyCard
    Task<GetItemResponse?> GetLoyaltyCardAsync(Guid userId, Guid businessId);
    Task DeleteLoyaltyCardAsync(Guid userId, Guid businessId);
    Task StampLoyaltyCardAsync(Dictionary<string, AttributeValue> item);
    
    // General
    Task WriteRecordAsync(Dictionary<string, AttributeValue> item, string? conditionExpression);
    Task WriteBatchAsync(List<Dictionary<string, AttributeValue>> itemList);
    Task UpdateRecordAsync(Dictionary<string, AttributeValue> item, string? conditionExpression);
    Task DeleteItemsWithPkAsync(string pk);
}