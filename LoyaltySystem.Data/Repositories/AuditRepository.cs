using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Extensions;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Settings;

namespace LoyaltySystem.Data.Repositories;

public class AuditRepository : IAuditRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDbSettings _dynamoDbSettings;

    public AuditRepository(IAmazonDynamoDB dynamoDb, DynamoDbSettings dynamoDbSettings)
    {
        _dynamoDb = dynamoDb;
        _dynamoDbSettings = dynamoDbSettings;
    }
    
    public async Task CreateAuditRecordAsync<T>(AuditRecord auditRecord)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "PK", new AttributeValue { S = $"{auditRecord.EntityType}#{auditRecord.EntityId}" } },
            { "SK", new AttributeValue { S = $"Action#{auditRecord.EntityType}#{auditRecord.AuditId}" } },

            { "AuditId",         new AttributeValue { S = $"{auditRecord.AuditId}" }},
            { "EntityId",        new AttributeValue { S = $"{auditRecord.EntityId}" }},
            { "EntityType",      new AttributeValue { S = $"{auditRecord.EntityType}" }},
            { "ActionType",      new AttributeValue { S = $"{auditRecord.ActionType}" }},
            { "ActionDetails",   new AttributeValue { S = auditRecord.ActionDetails }},
            { "Timestamp",       new AttributeValue { S = auditRecord.Timestamp.ToString("dd/MM/yyyy HH:mm:ss") }},
        };

        if (auditRecord.UserId.HasValue) 
            item["UserId"] = new AttributeValue { S = $"{auditRecord.UserId}" };

        if (!string.IsNullOrEmpty(auditRecord.Source))
            item["Source"] = new AttributeValue { S = auditRecord.Source };

        var request = new PutItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Item = item,
            ConditionExpression = "attribute_not_exists(PK)"
        };

        try
        {
            await _dynamoDb.PutItemAsync(request);
            Console.WriteLine($"Audit Record Written - {auditRecord.AuditId}");
        }
        catch (ConditionalCheckFailedException)
        {
            throw new Exception($"Audit record with AuditId {auditRecord.AuditId} is already in use.");
        }
    }
}