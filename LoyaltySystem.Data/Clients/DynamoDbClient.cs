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
        var request = new DeleteItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "PK", new AttributeValue { S = $"Business#{businessId}" }},
                { "SK", new AttributeValue { S = "Meta#BusinessInfo"   }}
            }
        };

        try
        {
            var response = await _dynamoDb.DeleteItemAsync(request);
        }
        catch (ConditionalCheckFailedException)
        {
            throw new Exception($"Failed to delete item with PK - Business#{businessId}; SK - Meta#BusinessInfo due to condition check");
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
}