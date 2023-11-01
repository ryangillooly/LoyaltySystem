using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Models;
using static LoyaltySystem.Core.Models.Constants;

namespace LoyaltySystem.Core.Utilities;

public static class DynamoDbValidator
{
    public static void ValidateUser(this Dictionary<string, AttributeValue> item)
    {
        if (!item.ContainsKey(UserId))    throw new KeyNotFoundException($"{UserId} is required for mapping.");
        if (!item.ContainsKey(FirstName)) throw new KeyNotFoundException($"{FirstName} is required for mapping.");
        if (!item.ContainsKey(LastName))  throw new KeyNotFoundException($"{LastName} is required for mapping.");
        if (!item.ContainsKey(Status))    throw new KeyNotFoundException($"{Status} is required for mapping.");
        if (!item.ContainsKey(Email))     throw new KeyNotFoundException($"{Email} is required for mapping.");
    }
}