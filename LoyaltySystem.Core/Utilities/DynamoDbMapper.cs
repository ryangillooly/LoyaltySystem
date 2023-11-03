using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using static LoyaltySystem.Core.Models.Constants;
using Newtonsoft.Json;
using EntityType = LoyaltySystem.Core.Enums.EntityType;

namespace LoyaltySystem.Core.Utilities;

public static class DynamoDbMapper 
{
    // Users
    public static Dictionary<string, AttributeValue> MapUserToItem(this User user)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            // Primary Key + Sort Key
            { Pk,          new AttributeValue { S = UserPrefix + user.Id } },
            { Sk,          new AttributeValue { S = MetaUserInfo } },
            
            // Attributes
            { UserId,                   new AttributeValue { S = user.Id.ToString() } },
            { Email,                    new AttributeValue { S = user.ContactInfo.Email } },
            { FirstName,                new AttributeValue { S = user.FirstName } },
            { LastName,                 new AttributeValue { S = user.LastName } },
            { Status,                   new AttributeValue { S = user.Status.ToString() } },
            { EntityTypeAttributeName,  new AttributeValue { S = user.GetType().Name } }
        };

        if (!string.IsNullOrWhiteSpace(user.ContactInfo.PhoneNumber))
            item[PhoneNumber] = new AttributeValue { S = user.ContactInfo.PhoneNumber };
            
        if (user.DateOfBirth.HasValue)
            item[DateOfBirth] = new AttributeValue { S = user.DateOfBirth.Value.ToString("yyyy-MM-dd") };
        
        return item;
    }
    public static User MapItemToUser(this Dictionary<string, AttributeValue> item)
    {
        item.ValidateUser();

        var user = new User
        {
            Id           = Guid.Parse(item[UserId].S),
            FirstName    = item[FirstName].S,
            LastName     = item[LastName].S,
            Status       = Enum.Parse<UserStatus>(item[Status].S),
            ContactInfo = new ContactInfo
            {
                Email = item[Email].S,
            }
        };

        if (item.TryGetValue(DateOfBirth, out var dateOfBirth)) user.DateOfBirth = DateTime.Parse(dateOfBirth.S);
        if (item.TryGetValue(PhoneNumber, out var phoneNumber)) user.ContactInfo.PhoneNumber = phoneNumber.S;

        return user;
    }
    
    
    // Businesses
    public static Dictionary<string, AttributeValue> MapBusinessToItem(this Business business)
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
    public static Business MapItemToBusiness(this Dictionary<string, AttributeValue> item)
    {
        item.ValidateBusiness();
        
        var business = new Business
        {
            Id           = Guid.Parse(item[BusinessId].S),
            OwnerId      = Guid.Parse(item[OwnerId].S),
            Name         = item[Name].S,
            Location     = JsonConvert.DeserializeObject<Location>(item[LocationAttributeName].S)          ?? throw new NullReferenceException(),
            OpeningHours = JsonConvert.DeserializeObject<OpeningHours>(item[OpeningHoursAtttributeName].S) ?? throw new NullReferenceException(),
            ContactInfo  = new ContactInfo { Email = item[Email].S },
            Status       = Enum.Parse<BusinessStatus>(item[Status].S),
        };
        
        if (item.TryGetValue(Description, out var description)) business.Description = description.S;
        if (item.TryGetValue(PhoneNumber, out var phoneNumber)) business.ContactInfo.PhoneNumber = phoneNumber.S;
        
        return business;
    }

    
    // Business User Permissions
    public static List<Dictionary<string, AttributeValue>> MapBusinessUserPermissionsToItem(this List<BusinessUserPermissions> businessUserPermissions) =>
        businessUserPermissions.Select(permission => new Dictionary<string, AttributeValue>
            {
                // Primary Key + Sort Key
                { Pk, new AttributeValue { S = UserPrefix + permission.UserId }},
                { Sk, new AttributeValue { S = PermissionBusinessPrefix + permission.BusinessId }},
                
                // Attributes
                { UserId,                  new AttributeValue { S = $"{permission.UserId}" } },
                { BusinessId,              new AttributeValue { S = $"{permission.BusinessId}" } },
                { EntityTypeAttributeName, new AttributeValue { S = $"{EntityType.Permission}" } },
                { Role,                    new AttributeValue { S = $"{Enum.Parse<UserRole>(permission.Role.ToString())}" }},
                { Timestamp,               new AttributeValue { S = $"{DateTime.UtcNow}" } },
                { BusinessUserListPk,      new AttributeValue { S = $"{permission.BusinessId}" } },
                { BusinessUserListSk,      new AttributeValue { S = PermissionBusinessPrefix + permission.UserId }}
            })
            .ToList();
    public static List<BusinessUserPermissions> MapItemToBusinessUserPermissions(this List<Dictionary<string, AttributeValue>> items)
    {
        var businessUserPermissionsList = new List<BusinessUserPermissions>();
        
        foreach (var item in items)
        {
            item.ValidateBusinessUserPermissions();

            businessUserPermissionsList.Add
            (
                new BusinessUserPermissions
                (
                    Guid.Parse(item[BusinessId].S), 
                    Guid.Parse(item[UserId].S), 
                    Enum.Parse<UserRole>(item[Role].S)
                )
            );
        }

        return businessUserPermissionsList;
    }
    
    
    // Business Campaigns
    public static Dictionary<string, AttributeValue> MapCampaignToItem(this Campaign campaign) =>
        new ()
        {
            // Primary Key + Sort Key
            { Pk, new AttributeValue { S = BusinessPrefix + campaign.BusinessId }},
            { Sk, new AttributeValue { S = CampaignPrefix + campaign.Id }},

            // Attributes
            { BusinessId,              new AttributeValue { S = $"{campaign.BusinessId}" }},
            { EntityTypeAttributeName, new AttributeValue { S = campaign.GetType().Name }},
            { Name,       new AttributeValue { S = $"{campaign.Name}" }},
            { CampaignId, new AttributeValue { S = $"{campaign.Id}" }},
            { Rewards,    new AttributeValue { S = $"{JsonConvert.SerializeObject(campaign.Rewards)}" }},
            { StartTime,  new AttributeValue { S = $"{campaign.StartTime}" }},
            { EndTime,    new AttributeValue { S = $"{campaign.EndTime}" }},
            { IsActive,   new AttributeValue { BOOL = campaign.IsActive }},
        };
    public static Campaign MapItemToCampaign(this Dictionary<string, AttributeValue> item) =>
        new ()
        {
            Id         = Guid.Parse(item[CampaignId].S),
            BusinessId = Guid.Parse(item[BusinessId].S),
            Name       = item[Name].S,
            Rewards    = JsonConvert.DeserializeObject<List<Reward>>(item[Rewards].S), 
            StartTime  = DateTime.Parse(item[StartTime].S),
            EndTime    = DateTime.Parse(item[EndTime].S),
            IsActive   = item[IsActive].BOOL
        };
    
    
    // Loyalty Cards
    public static Dictionary<string, AttributeValue> MapLoyaltyCardToItem(this LoyaltyCard loyaltyCard)
    {
        var item = new Dictionary<string, AttributeValue>()
        {
            // Primary Key + Sort Key
            { Pk, new AttributeValue { S = UserPrefix + loyaltyCard.UserId }},
            { Sk, new AttributeValue { S = CardBusinessPrefix + loyaltyCard.BusinessId }},

            // Attributes
            { CardId,                  new AttributeValue { S = $"{loyaltyCard.Id}" } },
            { BusinessId,              new AttributeValue { S = $"{loyaltyCard.BusinessId}" } },
            { UserId,                  new AttributeValue { S = $"{loyaltyCard.UserId}" } },
            { EntityTypeAttributeName, new AttributeValue { S = loyaltyCard.GetType().Name } },
            { Points,                  new AttributeValue { N = $"{loyaltyCard.Points}" } },
            { IssueDate,               new AttributeValue { S = $"{loyaltyCard.IssueDate}" } },
            { LastStampDate,           new AttributeValue { S = $"{loyaltyCard.LastStampedDate}" } },
            { Status,                  new AttributeValue { S = $"{loyaltyCard.Status}" } },

            { BusinessLoyaltyListPk, new AttributeValue { S = $"{loyaltyCard.BusinessId}" } },
            { BusinessLoyaltyListSk, new AttributeValue { S = CardUserPrefix + loyaltyCard.UserId + "#" + loyaltyCard.Id }}
        };

        if (loyaltyCard.LastUpdatedDate is not null)
            item.Add(LastUpdatedDate, new AttributeValue { S = $"{loyaltyCard.LastStampedDate}" });

        if(loyaltyCard.LastRedeemDate is not null)
            item.Add(LastRedeemDate, new AttributeValue { S = $"{loyaltyCard.LastRedeemDate}" });
        
        return item;
    }
    public static LoyaltyCard MapItemToLoyaltyCard(this Dictionary<string, AttributeValue> item)
    {
        var loyaltyCard = new LoyaltyCard
        {
            Id              = Guid.Parse(item[CardId].S),
            BusinessId      = Guid.Parse(item[BusinessId].S),
            UserId          = Guid.Parse(item[UserId].S),
            Points          = int.Parse(item[Points].N),
            IssueDate       = DateTime.Parse(item[IssueDate].S),
            LastStampedDate = DateTime.Parse(item[LastStampDate].S),
            Status          = Enum.Parse<LoyaltyStatus>(item[Status].S)
        };
        
        if (item.TryGetValue(LastRedeemDate,  out var lastRedeemDate))  loyaltyCard.LastRedeemDate  = Convert.ToDateTime(lastRedeemDate.S);
        if (item.TryGetValue(LastUpdatedDate, out var lastUpdatedDate)) loyaltyCard.LastUpdatedDate = Convert.ToDateTime(lastUpdatedDate.S);

        return loyaltyCard;
    }
    public static Dictionary<string, AttributeValue> MapLoyaltyCardToStampItem(this LoyaltyCard loyaltyCard)
    {
        var stampId = Guid.NewGuid();
        return new Dictionary<string, AttributeValue>
        {
            // Primary Key + Sort Key
            { Pk, new AttributeValue { S = UserPrefix + loyaltyCard.UserId }},
            { Sk, new AttributeValue { S = ActionStampBusinessPrefix + loyaltyCard.BusinessId + "#" + stampId }},

            // Attributes
            { CardId,     new AttributeValue { S = $"{loyaltyCard.Id}" }},
            { BusinessId, new AttributeValue { S = $"{loyaltyCard.BusinessId}" }},
            { UserId,     new AttributeValue { S = $"{loyaltyCard.UserId}" }},
            { StampId,    new AttributeValue { S = $"{stampId}" }},
            { EntityTypeAttributeName, new AttributeValue { S = "Stamp" }},
            { StampDate,  new AttributeValue { S = $"{loyaltyCard.LastStampedDate}" }}
        };
    }
    public static Dictionary<string, AttributeValue> MapLoyaltyCardToRedeemItem(this LoyaltyCard loyaltyCard, Guid campaignId, Guid rewardId)
    {
        var redeemId = Guid.NewGuid();
        return new Dictionary<string, AttributeValue>
            {
                // Primary Key + Sort Key
                { Pk, new AttributeValue { S = UserPrefix + loyaltyCard.UserId }},
                { Sk, new AttributeValue { S = ActionRedeemBusinessPrefix + loyaltyCard.BusinessId + "#" + redeemId }},

                // Attributes
                { UserId,                  new AttributeValue { S = $"{loyaltyCard.UserId}" } },
                { BusinessId,              new AttributeValue { S = $"{loyaltyCard.BusinessId}" } },
                { CampaignId,              new AttributeValue { S = $"{campaignId}" } },
                { CardId,                  new AttributeValue { S = $"{loyaltyCard.Id}" } },
                { RewardId,                new AttributeValue { S = $"{rewardId}" } },
                { EntityTypeAttributeName, new AttributeValue { S = "Redeem" } },
                { RedeemDate,              new AttributeValue { S = $"{loyaltyCard.LastRedeemDate}" } }
            };
    }

}