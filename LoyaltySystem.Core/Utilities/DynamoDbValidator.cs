using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Models;
using static LoyaltySystem.Core.Models.Constants;

namespace LoyaltySystem.Core.Utilities;

public static class DynamoDbValidator
{
    public static void ValidateUser(this Dictionary<string, AttributeValue> item)
    {
        if (!item.ContainsKey(UserId)) throw new KeyNotFoundException($"{UserId} is required for mapping.");
        if (!item.ContainsKey(FirstName)) throw new KeyNotFoundException($"{FirstName} is required for mapping.");
        if (!item.ContainsKey(LastName)) throw new KeyNotFoundException($"{LastName} is required for mapping.");
        if (!item.ContainsKey(Status)) throw new KeyNotFoundException($"{Status} is required for mapping.");
        if (!item.ContainsKey(Email)) throw new KeyNotFoundException($"{Email} is required for mapping.");
    }

    public static void ValidateBusiness(this Dictionary<string, AttributeValue> item)
    {
        if (!item.ContainsKey(BusinessId)) throw new KeyNotFoundException($"{BusinessId} is required for mapping.");
        if (!item.ContainsKey(OwnerId)) throw new KeyNotFoundException($"{OwnerId} is required for mapping.");
        if (!item.ContainsKey(Name)) throw new KeyNotFoundException($"{Name} is required for mapping.");
        if (!item.ContainsKey(Status)) throw new KeyNotFoundException($"{Status} is required for mapping.");
        if (!item.ContainsKey(Email)) throw new KeyNotFoundException($"{Email} is required for mapping.");
        if (!item.ContainsKey(OpeningHoursAttName))
            throw new KeyNotFoundException($"{OpeningHoursAttName} is required for mapping.");
        if (!item.ContainsKey(LocationAttributeName))
            throw new KeyNotFoundException($"{LocationAttributeName} is required for mapping.");
    }

    public static void ValidateBusinessUserPermissions(this Dictionary<string, AttributeValue> item)
    {
        if (!item.ContainsKey(Constants.BusinessId))
            throw new KeyNotFoundException($"{BusinessId} is required for mapping.");
        if (!item.ContainsKey(UserId)) throw new KeyNotFoundException($"{UserId} is required for mapping.");
        if (!item.ContainsKey(Role)) throw new KeyNotFoundException($"{Role} is required for mapping.");
    }
}