using System.Collections;
using Amazon.DynamoDBv2.Model;

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

}