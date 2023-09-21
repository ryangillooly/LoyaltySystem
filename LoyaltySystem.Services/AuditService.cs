using System.Globalization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Settings;
using LoyaltySystem.Core.Extensions;

namespace LoyaltySystem.Services;

public class AuditService : IAuditService
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDbSettings _dynamoDbSettings;

    public AuditService(IAmazonDynamoDB dynamoDb, DynamoDbSettings dynamoDbSettings)
    {
        _dynamoDb = dynamoDb;
        _dynamoDbSettings = dynamoDbSettings;
    }

    public async Task CreateAuditRecordAsync<T>
    (
        Guid entityId, 
        InteractionType interactionType, 
        string? interactionId = null
    )
    {
        interactionId ??= Guid.NewGuid().ToString();
        
        var item = new Dictionary<string, AttributeValue>
        {
            { "PK", new AttributeValue { S = $"{typeof(T).Name.ToPascalCase()}#{entityId}" } },
            { "SK", new AttributeValue { S = $"Action#{typeof(T).Name.ToPascalCase()}#{interactionType}" } },

            { "ActionType", new AttributeValue { S = typeof(T).Name.ToPascalCase() }},
            { "EntityType", new AttributeValue { S = "Action" } },
            { "InteractionId", new AttributeValue { S = interactionId } },
            { "InteractionTime", new AttributeValue { S = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss") } },
            { "InteractionType", new AttributeValue { S = interactionType.ToString().ToPascalCase() }},
        };
        
        if (typeof(T).Name.ToPascalCase() == "User")
            item["UserId"] = new AttributeValue { S = entityId.ToString() };

        if (typeof(T).Name.ToPascalCase() == "Business")
            item["BusinessId"] = new AttributeValue { S = entityId.ToString() };

        var request = new PutItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Item = item
        };

        try
        {
           await _dynamoDb.PutItemAsync(request);
        }
        catch (ConditionalCheckFailedException)
        {
            throw new Exception($"Audit record with InteractionId {interactionId} is already in use.");
        }
    }
}
