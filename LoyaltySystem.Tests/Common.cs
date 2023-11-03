using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Models;
using Newtonsoft.Json;
using static LoyaltySystem.Core.Models.Constants;

namespace LoyaltySystem.Tests.Common;

public static class TestDataFactory
{
    public static Dictionary<string, AttributeValue> CreateUserItem(this User user)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            // Primary Key + Sort Key
            { Pk, new AttributeValue { S = UserPrefix + user.Id } },
            { Sk, new AttributeValue { S = MetaUserInfo } },

            // Attributes
            { UserId, new AttributeValue { S = user.Id.ToString() } },
            { Email, new AttributeValue { S = user.ContactInfo.Email } },
            { FirstName, new AttributeValue { S = user.FirstName } },
            { LastName, new AttributeValue { S = user.LastName } },
            { Status, new AttributeValue { S = user.Status.ToString() } },
            { EntityTypeAttributeName, new AttributeValue { S = user.GetType().Name } }
        };
        
        if (!string.IsNullOrWhiteSpace(user.ContactInfo.PhoneNumber))
            item[PhoneNumber] = new AttributeValue { S = user.ContactInfo.PhoneNumber };
            
        if (user.DateOfBirth.HasValue)
            item[DateOfBirth] = new AttributeValue { S = user.DateOfBirth.Value.ToString("yyyy-MM-dd") };

        return item;
    }
    
    public static Dictionary<string, AttributeValue> CreateBusinessItem(this Business business)
    {
        var openingHoursJson = JsonConvert.SerializeObject(business.OpeningHours);
        var locationJson     = JsonConvert.SerializeObject(business.Location);
        
        var item = new Dictionary<string, AttributeValue>
        {
            // Primary Key + Sort Key
            { Pk, new AttributeValue { S = BusinessPrefix + business.Id }},
            { Sk, new AttributeValue { S = MetaBusinessInfo }},
         
            // Attributes
            { BusinessId,                 new AttributeValue { S = business.Id.ToString()} },
            { OwnerId,                    new AttributeValue { S = business.OwnerId.ToString() }},
            { EntityTypeAttributeName,    new AttributeValue { S = business.GetType().Name} },
            { Name,                       new AttributeValue { S = business.Name }},
            { OpeningHoursAtttributeName, new AttributeValue { S = openingHoursJson }},
            { LocationAttributeName,      new AttributeValue { S = locationJson }},
            { Email,                      new AttributeValue { S = business.ContactInfo.Email }},
            { Status,                     new AttributeValue { S = business.Status.ToString() }},
        };

        if (!string.IsNullOrWhiteSpace(business.Description))             item[Description] = new AttributeValue { S = business.Description };
        if (!string.IsNullOrWhiteSpace(business.ContactInfo.PhoneNumber)) item[PhoneNumber] = new AttributeValue { S = business.ContactInfo.PhoneNumber };

        return item;
    }
}