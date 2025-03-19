using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Settings;

namespace LoyaltySystem.Data.Repositories;

public class EmailRepository : IEmailRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDbSettings _dynamoDbSettings;

    public EmailRepository(IAmazonDynamoDB dynamoDb, DynamoDbSettings dynamoDbSettings)
        => (_dynamoDb, _dynamoDbSettings) = (dynamoDb, dynamoDbSettings);
    
    public async Task<bool> DoesEmailExistAsync(string email)
    {
        var request = new QueryRequest
        {
            TableName = _dynamoDbSettings.TableName,
            IndexName = _dynamoDbSettings.EmailGsi,                           // Use GSI for querying
            KeyConditionExpression = "Email = :emailValue", // Assuming your GSI PK is named "Email"
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                {":emailValue", new AttributeValue { S = email }}
            },
            Limit = 1                                       // We only need to know if at least one item exists
        };

        var response = await _dynamoDb.QueryAsync(request);
        return response.Count > 0; // If count > 0, email exists
    }
}