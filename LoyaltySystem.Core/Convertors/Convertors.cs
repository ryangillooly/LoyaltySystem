using Newtonsoft.Json.Converters;
using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Models;
using Newtonsoft.Json;

namespace LoyaltySystem.Core;

public static class Convertors
{
    public static User ConvertFromDynamoItemToUser(Dictionary<string, AttributeValue> item)
    {
        var user = new User
        {
            Id           = Guid.Parse(item["UserId"].S),
            FirstName    = item["FirstName"].S,
            LastName     = item["LastName"].S,
            DateOfBirth  = DateTime.Parse(item["DateOfBirth"].S),
            Status       = Enum.Parse<UserStatus>(item["Status"].S),
            ContactInfo = new ContactInfo
            {
                Email       = item["EntityType"].S,
                PhoneNumber = item["PhoneNumber"].S
            }
        };

        if (item.TryGetValue("DateOfBirth", out var dateOfBirth))  user.DateOfBirth = DateTime.Parse(dateOfBirth.S);

        return user;
    }
    
    public static Business ConvertFromDynamoItemToBusiness(Dictionary<string, AttributeValue> item)
    {
        var business = new Business
        {
            Id           = Guid.Parse(item["BusinessId"].S),
            OwnerId      = Guid.Parse(item["OwnerId"].S),
            Name         = item["Name"].S,
            Location     = JsonConvert.DeserializeObject<Location>(item["Location"].S)         ?? throw new NullReferenceException(),
            OpeningHours = JsonConvert.DeserializeObject<OpeningHours>(item["OpeningHours"].S) ?? throw new NullReferenceException(),
            ContactInfo  = new ContactInfo { Email = item["EntityType"].S },
            Status       = Enum.Parse<BusinessStatus>(item["Status"].S),
        };
        
        if (item.TryGetValue("Desc",        out var description)) business.Description = description.S;
        if (item.TryGetValue("PhoneNumber", out var phoneNumber)) business.ContactInfo.PhoneNumber = phoneNumber.S;
        
        return business;
    }
    
    public static LoyaltyCard ConvertFromDynamoItemToLoyaltyCard(this Dictionary<string, AttributeValue> item)
    {
        var loyaltyCard = new LoyaltyCard
        {
            Id              = Guid.Parse(item["CardId"].S),
            BusinessId      = Guid.Parse(item["BusinessId"].S),
            UserId          = Guid.Parse(item["UserId"].S),
            Points          = int.Parse(item["Points"].N),
            IssueDate       = DateTime.Parse(item["IssueDate"].S),
            LastStampedDate = DateTime.Parse(item["LastStampDate"].S),
            Status          = Enum.Parse<LoyaltyStatus>(item["Status"].S)
        };
        
        if (item.TryGetValue("LastRedeemDate",  out var lastRedeemDate)) loyaltyCard.LastRedeemDate = Convert.ToDateTime(lastRedeemDate.S);
        if (item.TryGetValue("LastUpdatedDate", out var lastUpdatedDate)) loyaltyCard.LastUpdatedDate = Convert.ToDateTime(lastUpdatedDate.S);

        return loyaltyCard;
    }
}