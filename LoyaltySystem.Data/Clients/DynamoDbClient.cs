using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Settings;

namespace LoyaltySystem.Data.Clients;

public interface IDynamoDbClient
{
    Task WriteRecordAsync(Dictionary<string, AttributeValue> item, string? conditionExpression);
    Task<GetItemResponse> GetUserByIdAsync(Guid userId);
    Task<GetItemResponse> GetBusinessByIdAsync(Guid businessId);
    Task DeleteBusinessAsync(Guid businessId);
    Task DeleteUserAsync(Guid userId);
    Task UpdateRecordAsync(Dictionary<string, AttributeValue> item, string? conditionExpression);
}

public class DynamoDbClient : IDynamoDbClient
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDbSettings _dynamoDbSettings;

    public DynamoDbClient(IAmazonDynamoDB dynamoDb, DynamoDbSettings dynamoDbSettings) =>
        (_dynamoDb, _dynamoDbSettings) = (dynamoDb, dynamoDbSettings);

    public async Task<GetItemResponse?> GetUserByIdAsync(Guid userId)
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

        var response = await _dynamoDb.GetItemAsync(request);

        if (response.Item == null || !response.IsItemSet)
            return null;

        return response;
    }
    
    public async Task<GetItemResponse?> GetBusinessByIdAsync(Guid businessId)
    {
        var request = new GetItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "PK", new AttributeValue { S = $"Business#{businessId}" }},
                { "SK", new AttributeValue { S = "Meta#BusinessInfo"  }}
            }
        };

        var response = await _dynamoDb.GetItemAsync(request);

        if (response.Item == null || !response.IsItemSet)
            return null;

        return response;
    }
    
    public async Task WriteRecordAsync(Dictionary<string, AttributeValue> item, string? conditionExpression)
    {
        var request = new PutItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Item = item
        };

        if (conditionExpression is not null)
            request.ConditionExpression = conditionExpression;

        try
        {
            var response = await _dynamoDb.PutItemAsync(request);
        }
        catch (ConditionalCheckFailedException)
        {
            throw new Exception($"PK - {item["PK"].S}; SK - {item["SK"].S} is already in use");
        }
    }
    
    public async Task DeleteBusinessAsync(Guid businessId)
    {
        try
        {
            await DeleteItemsWithPK($"Business#{businessId}");
        }
        catch (ConditionalCheckFailedException)
        {
            throw new Exception($"Failed to delete item with PK - Business#{businessId} due to condition check");
        }
    }
    
    public async Task DeleteUserAsync(Guid userId)
    {
        try
        {
            await DeleteItemsWithPK($"User#{userId}");
        }
        catch (ConditionalCheckFailedException)
        {
            throw new Exception($"Failed to delete item with PK - User#{userId} due to condition check");
        }
    }

    public async Task UpdateRecordAsync(Dictionary<string, AttributeValue> item, string? conditionExpression)
    {
        var request = new PutItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Item = item
        };

        await _dynamoDb.PutItemAsync(request);
    }
    
    public async Task<List<string>> GetAllSortKeysForPartitionKey(string PK)
    {
        var queryRequest = new QueryRequest
        {
            TableName = _dynamoDbSettings.TableName,
            KeyConditionExpression = "PK = :PK",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                {":PK", new AttributeValue {S = PK}}
            }
        };

        var response = await _dynamoDb.QueryAsync(queryRequest);
    
        // Extract sort keys (SKs) from the response
        return response.Items.Select(item => item["SK"].S).ToList();
    }
    
    public async Task DeleteItemsWithPK(string PK)
    {
        var sortKeys = await GetAllSortKeysForPartitionKey(PK);

        // Create batch delete requests
        var batchRequests = new List<WriteRequest>();
        foreach (var SK in sortKeys)
        {
            batchRequests.Add(new WriteRequest
            {
                DeleteRequest = new DeleteRequest
                {
                    Key = new Dictionary<string, AttributeValue>
                    {
                        {"PK", new AttributeValue {S = PK}},
                        {"SK", new AttributeValue {S = SK}}
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

            await _dynamoDb.BatchWriteItemAsync(batchWriteItemRequest);
        }
    }

}