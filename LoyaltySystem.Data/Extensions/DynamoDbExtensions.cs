using System.Collections;
using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Settings;

namespace LoyaltySystem.Data.Extensions;

public static class DynamoDbExtensions
{
    public static Dictionary<string, IList> ToModelLists
    (
        this TransactGetItemsResponse response,
        Dictionary<string, Func<Dictionary<string, AttributeValue>, object>> converters
    )
    {
        var result = new Dictionary<string, IList>();

        foreach (var entityType in converters.Keys)
        {
            result[entityType] = new List<object>();
        }

        foreach (var getResult in response.Responses)
        {
            var item = getResult.Item;
            string entityType = item["EntityType"].S;

            if (converters.TryGetValue(entityType, out var converter))
            {
                var convertedItem = converter(item);
                ((List<object>)result[entityType]).Add(convertedItem);
            }
            else
            {
                // Handle or throw an exception for unexpected entity types
            }
        }

        return result;
    }

    public static TransactGetItem ToTransactGetItem(DynamoDbSettings dynamoDbSettings, string pkValue, string skValue)
    {
        return new TransactGetItem
        {
            Get = new Get
            {
                TableName = dynamoDbSettings.TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "PK", new AttributeValue { S = pkValue } },
                    { "SK", new AttributeValue { S = skValue } }
                }
            }
        };
    }
}

