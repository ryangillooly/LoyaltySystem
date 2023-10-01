using Amazon.DynamoDBv2.Model;

namespace LoyaltySystem.Core.Interfaces;

public interface IDynamoDbClient
{
    Task<GetItemResponse> GetUserAsync(Guid userId);
    Task<GetItemResponse> GetBusinessAsync(Guid businessId);
    Task<GetItemResponse> GetLoyaltyCardAsync(Guid userId, Guid businessId);
    Task<GetItemResponse> GetCampaignAsync(Guid businessId, Guid campaignId);
    Task WriteRecordAsync(Dictionary<string, AttributeValue> item, string? conditionExpression);
    Task UpdateRecordAsync(Dictionary<string, AttributeValue> item, string? conditionExpression);
    Task DeleteLoyaltyCardAsync(Guid userId, Guid businessId);
    Task DeleteItemsWithPkAsync(string pk);
    Task DeleteCampaignAsync(Guid businessId, Guid campaignId);
}