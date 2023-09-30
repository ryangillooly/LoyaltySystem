using Amazon.DynamoDBv2.Model;

namespace LoyaltySystem.Core.Interfaces;

public interface IDynamoDbClient
{
    Task WriteRecordAsync(Dictionary<string, AttributeValue> item, string? conditionExpression);
    Task<GetItemResponse> GetUserAsync(Guid userId);
    Task<GetItemResponse> GetBusinessAsync(Guid businessId);
    Task DeleteLoyaltyCardAsync(Guid userId, Guid businessId);
    Task DeleteItemsWithPkAsync(string pk);
    Task UpdateRecordAsync(Dictionary<string, AttributeValue> item, string? conditionExpression);
    Task<GetItemResponse> GetLoyaltyCardAsync(Guid userId, Guid businessId);
}