using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using Newtonsoft.Json;

namespace LoyaltySystem.Core.Utilities;

public class DynamoDbMapper : IDynamoDbMapper
{
    public Dictionary<string, AttributeValue> MapUserToItem(User user)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "PK",          new AttributeValue { S = "User#" + user.Id } },
            { "SK",          new AttributeValue { S = "Meta#UserInfo" } },
            { "UserId",      new AttributeValue { S = user.Id.ToString() } },
            { "EntityType",  new AttributeValue { S = user.GetType().Name } },
            { "Email",       new AttributeValue { S = user.ContactInfo.Email } },
            { "PhoneNumber", new AttributeValue { S = user.ContactInfo.PhoneNumber } },
            { "FirstName",   new AttributeValue { S = user.FirstName } },
            { "LastName",    new AttributeValue { S = user.LastName } },
            { "Status",      new AttributeValue { S = user.Status.ToString() } },
        };

        if (user.DateOfBirth.HasValue)
            item["DateOfBirth"] = new AttributeValue { S = user.DateOfBirth.Value.ToString("yyyy-MM-dd") };
        
        return item;
    }

    public List<Dictionary<string, AttributeValue>> MapBusinessUserPermissionsToItem(BusinessUserPermissions businessUserPermissions) =>
        businessUserPermissions.Permissions.Select
        (
            permission => new Dictionary<string, AttributeValue>
            {
                { "PK",                  new AttributeValue { S = $"User#{permission.UserId}" } },
                { "SK",                  new AttributeValue { S = $"Permission#Business#{businessUserPermissions.BusinessId}" } },
                { "UserId",              new AttributeValue { S = $"{permission.UserId}" } },
                { "BusinessId",          new AttributeValue { S = $"{businessUserPermissions.BusinessId}" } },
                { "EntityType",          new AttributeValue { S = $"{EntityType.Permission}" } },
                { "Role",                new AttributeValue { S = $"{permission.Role}" } },
                { "Timestamp",           new AttributeValue { S = $"{DateTime.UtcNow}" } },
                { "BusinessUserList-PK", new AttributeValue { S = $"{businessUserPermissions.BusinessId}" } },
                { "BusinessUserList-SK", new AttributeValue { S = $"Permission#User#{permission.UserId}" } }
            }
        )
        .ToList();

    public Dictionary<string, AttributeValue> MapLoyaltyCardToItem(LoyaltyCard loyaltyCard) =>
        new ()
        {
            // New PK and SK patterns
            { "PK", new AttributeValue { S = $"User#{loyaltyCard.UserId}" }},
            { "SK", new AttributeValue { S = $"Card#Business#{loyaltyCard.BusinessId}" }},

            // New Type attribute
            { "CardId",        new AttributeValue { S = $"{loyaltyCard.Id}" }},
            { "BusinessId",    new AttributeValue { S = $"{loyaltyCard.BusinessId}" }},
            { "UserId",        new AttributeValue { S = $"{loyaltyCard.UserId}" }},
            { "EntityType",    new AttributeValue { S = loyaltyCard.GetType().Name }},
            { "Points",        new AttributeValue { N = $"{loyaltyCard.Points}" }},
            { "DateIssued",    new AttributeValue { S = $"{loyaltyCard.DateIssued}" }},
            { "LastStampDate", new AttributeValue { S = $"{loyaltyCard.DateLastStamped}" }},
            { "Status",        new AttributeValue { S = $"{loyaltyCard.Status}" }},

            { "BusinessLoyaltyList-PK", new AttributeValue { S = $"{loyaltyCard.BusinessId}" } },
            { "BusinessLoyaltyList-SK", new AttributeValue { S = $"Card#{loyaltyCard.Id}" } }
        };

    public Dictionary<string, AttributeValue> MapBusinessToItem(Business business)
    {
        var openingHoursJson = JsonConvert.SerializeObject(business.OpeningHours);
        var locationJson     = JsonConvert.SerializeObject(business.Location);
        
        return new Dictionary<string, AttributeValue>
        {
            // New PK and SK patterns
            { "PK",          new AttributeValue { S = "Business#" + business.Id }},
            { "SK",          new AttributeValue { S = "Meta#BusinessInfo" }},
         
            // New Type attribute
            { "BusinessId",   new AttributeValue { S = business.Id.ToString()} },
            { "OwnerId",      new AttributeValue { S = business.OwnerId.ToString()} },
            { "EntityType",   new AttributeValue { S = business.GetType().Name} },
            { "Name",         new AttributeValue { S = business.Name }},
            { "OpeningHours", new AttributeValue { S = openingHoursJson }},
            { "Location",     new AttributeValue { S = locationJson }},
            { "Desc",         new AttributeValue { S = business.Description }},
            { "PhoneNumber",  new AttributeValue { S = business.ContactInfo.PhoneNumber }},
            { "Email",        new AttributeValue { S = business.ContactInfo.Email }},
            { "Status",       new AttributeValue { S = business.Status.ToString() }},
        };
    }

    public Dictionary<string, AttributeValue> MapCampaignToItem(Campaign campaign) =>
        new ()
        {
            // New PK and SK patterns
            { "PK", new AttributeValue { S = $"Business#{campaign.BusinessId}" }},
            { "SK", new AttributeValue { S = $"Campaign#{campaign.Id}" }},

            // New Type attribute
            { "BusinessId", new AttributeValue { S = $"{campaign.BusinessId}" }},
            { "EntityType", new AttributeValue { S = campaign.GetType().Name }},
            { "Name",       new AttributeValue { S = $"{campaign.Name}" }},
            { "CampaignId", new AttributeValue { S = $"{campaign.Id}" }},
            { "Rewards",    new AttributeValue { S = $"{JsonConvert.SerializeObject(campaign.Rewards)}" }},
            { "StartTime",  new AttributeValue { S = $"{campaign.StartTime}" }},
            { "EndTime",    new AttributeValue { S = $"{campaign.EndTime}" }},
            { "IsActive",   new AttributeValue { BOOL = campaign.IsActive }},
        };
}