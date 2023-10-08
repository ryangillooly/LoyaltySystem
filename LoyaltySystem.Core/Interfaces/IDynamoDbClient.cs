using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface IDynamoDbClient
{
    // General
    Task UpdateRecordAsync(Dictionary<string, AttributeValue> item, string? conditionExpression);
    Task DeleteItemsWithPkAsync(string pk);
    Task TransactWriteItemsAsync(List<TransactWriteItem> transactWriteItems);
    Task BatchWriteRecordsAsync(IEnumerable<Dictionary<string, AttributeValue>> items);
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