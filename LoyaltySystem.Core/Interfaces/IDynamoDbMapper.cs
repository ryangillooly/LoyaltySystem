using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface IDynamoDbMapper
{ 
    Dictionary<string, AttributeValue> MapUserToItem(User user);
    Dictionary<string, AttributeValue> MapBusinessToItem(Business business);
    Dictionary<string, AttributeValue> MapLoyaltyCardToItem(LoyaltyCard loyaltyCard);
    Dictionary<string, AttributeValue> MapLoyaltyCardToStampItem(LoyaltyCard loyaltyCard);
    List<Dictionary<string, AttributeValue>> MapBusinessUserPermissionsToItem(List<BusinessUserPermissions> businessUserPermissions);
    Dictionary<string, AttributeValue> MapCampaignToItem(Campaign campaign);
}