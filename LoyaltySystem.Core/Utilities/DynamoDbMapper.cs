using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Utilities;

public class DynamoDbMapper : IDynamoDbMapper
{
    public Dictionary<string, AttributeValue> MapUserToItem(User user)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "PK", new AttributeValue { S = "User#" + user.Id } },
            { "SK", new AttributeValue { S = "Meta#UserInfo" } },
            { "UserId", new AttributeValue { S = user.Id.ToString() } },
            { "EntityType", new AttributeValue { S = user.GetType().Name } },
            { "Email", new AttributeValue { S = user.ContactInfo.Email } },
            { "PhoneNumber", new AttributeValue { S = user.ContactInfo.PhoneNumber } },
            { "FirstName", new AttributeValue { S = user.FirstName } },
            { "LastName", new AttributeValue { S = user.LastName } },
            { "Status", new AttributeValue { S = user.Status.ToString() } },
        };

        if (user.DateOfBirth.HasValue)
            item["DateOfBirth"] = new AttributeValue { S = user.DateOfBirth.Value.ToString("yyyy-MM-dd") };
        
        return item;
    }
}