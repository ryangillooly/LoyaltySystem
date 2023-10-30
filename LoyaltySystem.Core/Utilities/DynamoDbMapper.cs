using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using static LoyaltySystem.Core.Models.Constants;
using Newtonsoft.Json;

namespace LoyaltySystem.Core.Utilities;

public class DynamoDbMapper : IDynamoDbMapper
{
    public Dictionary<string, AttributeValue> MapUserToItem(User user)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            // Primary Key + Sort Key
            { "PK",          new AttributeValue { S = UserPrefix + user.Id } },
            { "SK",          new AttributeValue { S = MetaUserInfo } },
            
            // Attributes
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

    public List<Dictionary<string, AttributeValue>> MapBusinessUserPermissionsToItem(List<BusinessUserPermissions> businessUserPermissions) =>
        businessUserPermissions.Select(permission => new Dictionary<string, AttributeValue>
            {
                // Primary Key + Sort Key
                { "PK",                  new AttributeValue { S = UserPrefix + permission.UserId }},
                { "SK",                  new AttributeValue { S = PermissionBusinessPrefix + permission.BusinessId }},
                
                // Attributes
                { "UserId",              new AttributeValue { S = $"{permission.UserId}" } },
                { "BusinessId",          new AttributeValue { S = $"{permission.BusinessId}" } },
                { "EntityType",          new AttributeValue { S = $"{EntityType.Permission}" } },
                { "Role",                new AttributeValue { S = $"{Enum.Parse<UserRole>(permission.Role.ToString())}" }},
                { "Timestamp",           new AttributeValue { S = $"{DateTime.UtcNow}" } },
                { "BusinessUserList-PK", new AttributeValue { S = $"{permission.BusinessId}" } },
                { "BusinessUserList-SK", new AttributeValue { S = PermissionBusinessPrefix + permission.UserId }}
            })
            .ToList();

    public Dictionary<string, AttributeValue> MapLoyaltyCardToItem(LoyaltyCard loyaltyCard)
    {
        var item = new Dictionary<string, AttributeValue>()
        {
            // Primary Key + Sort Key
            { "PK", new AttributeValue { S = UserPrefix + loyaltyCard.UserId }},
            { "SK", new AttributeValue { S = CardBusinessPrefix + loyaltyCard.BusinessId }},

            // Attributes
            { "CardId",        new AttributeValue { S = $"{loyaltyCard.Id}" } },
            { "BusinessId",    new AttributeValue { S = $"{loyaltyCard.BusinessId}" } },
            { "UserId",        new AttributeValue { S = $"{loyaltyCard.UserId}" } },
            { "EntityType",    new AttributeValue { S = loyaltyCard.GetType().Name } },
            { "Points",        new AttributeValue { N = $"{loyaltyCard.Points}" } },
            { "IssueDate",     new AttributeValue { S = $"{loyaltyCard.IssueDate}" } },
            { "LastStampDate", new AttributeValue { S = $"{loyaltyCard.LastStampedDate}" } },
            { "Status",        new AttributeValue { S = $"{loyaltyCard.Status}" } },

            { "BusinessLoyaltyList-PK", new AttributeValue { S = $"{loyaltyCard.BusinessId}" } },
            { "BusinessLoyaltyList-SK", new AttributeValue { S = CardUserPrefix + loyaltyCard.UserId + "#" + loyaltyCard.Id }}
        };

        if (loyaltyCard.LastUpdatedDate is not null)
            item.Add("LastUpdatedDate", new AttributeValue { S = $"{loyaltyCard.LastStampedDate}" });

        if(loyaltyCard.LastRedeemDate is not null)
            item.Add("LastRedeemDate", new AttributeValue { S = $"{loyaltyCard.LastRedeemDate}" });
        
        return item;
    }

    public Dictionary<string, AttributeValue> MapLoyaltyCardToStampItem(LoyaltyCard loyaltyCard)
    {
        var stampId = Guid.NewGuid();
        return 
        new()
        {
            // Primary Key + Sort Key
            { "PK", new AttributeValue { S = UserPrefix + loyaltyCard.UserId }},
            { "SK", new AttributeValue { S = ActionStampBusinessPrefix + loyaltyCard.BusinessId + "#" + stampId }},

            // Attributes
            { "CardId",     new AttributeValue { S = $"{loyaltyCard.Id}" }},
            { "BusinessId", new AttributeValue { S = $"{loyaltyCard.BusinessId}" }},
            { "UserId",     new AttributeValue { S = $"{loyaltyCard.UserId}" }},
            { "StampId",    new AttributeValue { S = $"{stampId}" }},
            { "EntityType", new AttributeValue { S = "Stamp" }},
            { "StampDate",  new AttributeValue { S = $"{loyaltyCard.LastStampedDate}" }}
        };
    }

    public Dictionary<string, AttributeValue> MapLoyaltyCardToRedeemItem(LoyaltyCard loyaltyCard, Guid campaignId, Guid rewardId)
    {
        var redeemId = Guid.NewGuid();
        return
            new()
            {
                // Primary Key + Sort Key
                { "PK", new AttributeValue { S = UserPrefix + loyaltyCard.UserId }},
                { "SK", new AttributeValue { S = ActionRedeemBusinessPrefix + loyaltyCard.BusinessId + "#" + redeemId }},

                // Attributes
                { "UserId",     new AttributeValue { S = $"{loyaltyCard.UserId}" } },
                { "BusinessId", new AttributeValue { S = $"{loyaltyCard.BusinessId}" } },
                { "CampaignId", new AttributeValue { S = $"{campaignId}" } },
                { "CardId",     new AttributeValue { S = $"{loyaltyCard.Id}" } },
                { "RewardId",   new AttributeValue { S = $"{rewardId}" } },
                { "EntityType", new AttributeValue { S = "Redeem" } },
                { "RedeemDate", new AttributeValue { S = $"{loyaltyCard.LastRedeemDate}" } }
            };
    }

    public Dictionary<string, AttributeValue> MapBusinessToItem(Business business)
    {
        var openingHoursJson = JsonConvert.SerializeObject(business.OpeningHours);
        var locationJson     = JsonConvert.SerializeObject(business.Location);
        
        return new Dictionary<string, AttributeValue>
        {
            // Primary Key + Sort Key
            { "PK",          new AttributeValue { S = BusinessPrefix + business.Id }},
            { "SK",          new AttributeValue { S = MetaBusinessInfo }},
         
            // Attributes
            { "BusinessId",   new AttributeValue { S = business.Id.ToString()} },
            { "OwnerId",      new AttributeValue { S = business.OwnerId.ToString() }},
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
            // Primary Key + Sort Key
            { "PK", new AttributeValue { S = BusinessPrefix + campaign.BusinessId }},
            { "SK", new AttributeValue { S = CampaignPrefix + campaign.Id }},

            // Attributes
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