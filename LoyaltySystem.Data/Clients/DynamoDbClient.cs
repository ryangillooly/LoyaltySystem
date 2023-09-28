using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using LoyaltySystem.Core.Settings;

namespace LoyaltySystem.Data.Clients;

public interface IDynamoDbClient
{
    Task WriteRecord(Dictionary<string, AttributeValue> item, string? conditionExpression);
    Task<GetItemResponse> GetUserById(Guid userId);
}

public class DynamoDbClient : IDynamoDbClient
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDbSettings _dynamoDbSettings;

    public DynamoDbClient(IAmazonDynamoDB dynamoDb, DynamoDbSettings dynamoDbSettings) =>
        (_dynamoDb, _dynamoDbSettings) = (dynamoDb, dynamoDbSettings);

    public async Task<GetItemResponse?> GetUserById(Guid userId)
    {
        var request = new GetItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Key = new Dictionary<string, AttributeValue> { { "UserId", new AttributeValue { S = userId.ToString() } } }
        };

        var response = await _dynamoDb.GetItemAsync(request);

        if (response.Item == null || !response.IsItemSet)
            return null;

        return response;
    }
    
    public async Task WriteRecord(Dictionary<string, AttributeValue> item, string? conditionExpression)
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
}