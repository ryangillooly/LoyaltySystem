using Amazon.DynamoDBv2.Model;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using FluentAssertions;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Models;
using static LoyaltySystem.Core.Models.Constants;
using LoyaltySystem.Core.Utilities;
using Newtonsoft.Json;
using LoyaltySystem.Tests.Common;
using Xunit;

namespace Tests.LoyaltySystem.Core.Utilities;

public class DynamoDbMapperTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    
    // MapUserToItem
    [Theory, AutoData]
    public void MapUserToItem_WithValidUser_MapsAllPropertiesCorrectly(User user)
    {
        // Arrange 
        user.Status = UserStatus.Active;
        
        // Act
        var result = user.MapUserToItem();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKeys(Pk, Sk, UserId, EntityTypeAttributeName, Email, PhoneNumber, FirstName, LastName, Status);
        
        result[Pk].S.Should().Be(UserPrefix + user.Id);
        result[Sk].S.Should().Be(MetaUserInfo);
        
        result[UserId].S.Should().Be(user.Id.ToString());
        result[EntityTypeAttributeName].S.Should().Be(EntityType.User.ToString());
        result[Email].S.Should().Be(user.ContactInfo.Email);
        result[PhoneNumber].S.Should().Be(user.ContactInfo.PhoneNumber);
        result[FirstName].S.Should().Be(user.FirstName);
        result[LastName].S.Should().Be(user.LastName);
        result[Status].S.Should().Be(UserStatus.Active.ToString());
        
        if (user.DateOfBirth.HasValue) 
            result[DateOfBirth].S.Should().Be(user.DateOfBirth.Value.ToString("yyyy-MM-dd"));
    }
    [Theory, AutoData]
    public void MapUserToItem_WithNullProperties_HandlesNullsAppropriately(User user)
    {
        // Arrange
        user.ContactInfo.PhoneNumber = null;
        user.DateOfBirth = null;
        
        // Act
        var result = user.MapUserToItem();

        // Assert
        result.TryGetValue(PhoneNumber, out var phoneNumber);
        result.TryGetValue(DateOfBirth, out var dateOfBirth);

        phoneNumber.Should().BeNull();
        dateOfBirth.Should().BeNull();
    }

    
    // MapItemToUser
    [Theory, AutoData]
    public void MapItemToUser_WithValidItem_MapsAllPropertiesCorrectly(User user)
    {
        // Arrange
        user.Status = UserStatus.Active;
        var userItem = user.CreateUserItem();
        
        // Act
        var result = userItem.MapItemToUser();
        
        // Assert
        result.Id.Should().Be(user.Id);
        result.ContactInfo.Email.Should().Be(user.ContactInfo.Email);
        result.ContactInfo.PhoneNumber.Should().Be(user.ContactInfo.PhoneNumber);
        result.FirstName.Should().Be(user.FirstName);
        result.LastName.Should().Be(user.LastName);
        result.Status.Should().Be(UserStatus.Active);
    }
    [Theory, AutoData]
    public void MapItemToUser_WithNullProperties_HandlesNullsAppropriately(User user)
    {
        // Arrange
        user.ContactInfo.PhoneNumber = null;
        user.DateOfBirth = null;
        
        var userItem = user.CreateUserItem();

        // Act
        var result = userItem.MapItemToUser();

        // Assert
        result.ContactInfo.PhoneNumber.Should().BeNull();
        result.DateOfBirth.Should().BeNull();
    }
    [Theory, AutoData]
    public void MapItemToUser_MissingKey_ThrowsKeyNotFoundException(User user)
    {
        // Arrange
        var userItem = user.CreateUserItem();
        userItem.Remove(Email); // Intentionally remove to test error handling

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => userItem.MapItemToUser());
    }
    
    
    
    
    // MapBusinessToItem
    [Theory, AutoData]
    public void MapBusinessToItem_WithValidBusiness_MapsAllPropertiesCorrectly(Business business)
    {
        // Arrange
        business.Status = BusinessStatus.Active;

        // Act
        var result = business.MapBusinessToItem();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKeys(Pk, Sk, BusinessId, OwnerId, EntityTypeAttributeName, Name, OpeningHoursAtttributeName, LocationAttributeName, Description, PhoneNumber, Email, Status);
        
        result[Pk].S.Should().Be(BusinessPrefix + business.Id);
        result[Sk].S.Should().Be(MetaBusinessInfo);
        
        result[BusinessId].S.Should().Be(business.Id.ToString());
        result[OwnerId].S.Should().Be(business.OwnerId.ToString());
        result[EntityTypeAttributeName].S.Should().Be(EntityType.Business.ToString());
        result[Name].S.Should().Be(business.Name);
        result[OpeningHoursAtttributeName].S.Should().Be(JsonConvert.SerializeObject(business.OpeningHours));
        result[LocationAttributeName].S.Should().Be(JsonConvert.SerializeObject(business.Location));
        result[Description].S.Should().Be(business.Description);
        result[PhoneNumber].S.Should().Be(business.ContactInfo.PhoneNumber);
        result[Email].S.Should().Be(business.ContactInfo.Email);
        result[Status].S.Should().Be(BusinessStatus.Active.ToString());
    }
    [Theory, AutoData]
    public void MapBusinessToItem_WithNullProperties_HandlesNullsAppropriately(Business business)
    {
        // Arrange
        business.ContactInfo.PhoneNumber = null;
        business.Description = null;
        
        // Act
        var result = business.MapBusinessToItem();

        // Assert
        result.TryGetValue(PhoneNumber, out var phoneNumber);
        result.TryGetValue(Description, out var description);

        phoneNumber.S.Should().BeNull();
        description.S.Should().BeNull();
    }

    
    
    
    
    // MapBusinessUserPermissionsToItem
    [Theory, AutoData]
    public void MapBusinessUserPermissionsToItem_WithValidPermissions_MapsAllPropertiesCorrectly(List<BusinessUserPermissions> permissions)
    {
        // Arrange
        permissions[0].Role = UserRole.Admin;
        
        // Act
        var result = permissions.MapBusinessUserPermissionsToItem();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().BeGreaterThan(0);
        
        foreach (var permission in permissions)
        {
            var mappedItem = result.Single(item => item[UserId].S == permission.UserId.ToString());
            mappedItem.Should().ContainKeys(Pk, Sk, UserId, BusinessId, EntityTypeAttributeName, Role, Timestamp, BusinessUserListPk, BusinessUserListSk);
           
            mappedItem[Pk].S.Should().Be(UserPrefix + permission.UserId);
            mappedItem[Sk].S.Should().Be(PermissionBusinessPrefix + permission.BusinessId);
            
            mappedItem[UserId].S.Should().Be(permission.UserId.ToString());
            mappedItem[BusinessId].S.Should().Be(permission.BusinessId.ToString());
            mappedItem[EntityTypeAttributeName].S.Should().Be(EntityType.Permission.ToString());
            mappedItem[Role].S.Should().Be(permission.Role.ToString());
            // mappedItem[Timestamp].S.Should().StartWith(DateTime.UtcNow.ToString("dd/MM/yyyy hh:mm"));
            
            mappedItem[BusinessUserListPk].S.Should().Be(permission.BusinessId.ToString());
            mappedItem[BusinessUserListSk].S.Should().Be(PermissionBusinessPrefix + permission.UserId);
        }
    }
    
    // Campaign
    [Fact]
    public void MapCampaignToItem_WithValidCampaign_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var campaign = _fixture.Create<Campaign>();

        // Act
        var result = campaign.MapCampaignToItem();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKeys("PK", "SK", "BusinessId", "EntityType", "Name", "CampaignId", "Rewards", "StartTime", "EndTime", "IsActive");
        
        result["PK"].S.Should().Be(BusinessPrefix + campaign.BusinessId);
        result["SK"].S.Should().Be(CampaignPrefix + campaign.Id);
        
        result["BusinessId"].S.Should().Be(campaign.BusinessId.ToString());
        result["EntityType"].S.Should().Be(campaign.GetType().Name);
        result["Name"].S.Should().Be(campaign.Name);
        result["CampaignId"].S.Should().Be(campaign.Id.ToString());
        result["Rewards"].S.Should().Be(JsonConvert.SerializeObject(campaign.Rewards));
        result["StartTime"].S.Should().Be(campaign.StartTime.ToString());
        result["EndTime"].S.Should().Be(campaign.EndTime.ToString());
        result["IsActive"].BOOL.Should().Be(campaign.IsActive);
    }
    
    // Loyalty Card
    [Fact]
    public void MapLoyaltyCardToItem_WithValidLoyaltyCard_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var loyaltyCard = _fixture.Create<LoyaltyCard>();

        // Act
        var result = loyaltyCard.MapLoyaltyCardToItem();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKeys("PK", "SK", "CardId", "BusinessId", "UserId", "EntityType", "Points", "IssueDate", "LastStampDate", "Status", "BusinessLoyaltyList-PK", "BusinessLoyaltyList-SK");
        
        result["PK"].S.Should().Be(Constants.UserPrefix + loyaltyCard.UserId);
        result["SK"].S.Should().Be(Constants.CardBusinessPrefix + loyaltyCard.BusinessId);
        
        result["CardId"].S.Should().Be(loyaltyCard.Id.ToString());
        result["BusinessId"].S.Should().Be(loyaltyCard.BusinessId.ToString());
        result["UserId"].S.Should().Be(loyaltyCard.UserId.ToString());
        result["EntityType"].S.Should().Be(EntityType.LoyaltyCard.ToString());
        result["Points"].N.Should().Be(loyaltyCard.Points.ToString());
        result["IssueDate"].S.Should().Be(loyaltyCard.IssueDate.ToString());
        result["LastStampDate"].S.Should().Be(loyaltyCard.LastStampedDate.ToString());
        result["Status"].S.Should().Be(loyaltyCard.Status.ToString());
        result["BusinessLoyaltyList-PK"].S.Should().Be(loyaltyCard.BusinessId.ToString());
        result["BusinessLoyaltyList-SK"].S.Should().Be(CardUserPrefix + loyaltyCard.UserId + "#" + loyaltyCard.Id);
        
        if (loyaltyCard.LastUpdatedDate.HasValue)
        {
            result.Should().ContainKey("LastUpdatedDate");
            result["LastUpdatedDate"].S.Should().Be(loyaltyCard.LastStampedDate.ToString());
        }

        if (loyaltyCard.LastRedeemDate.HasValue)
        {
            result.Should().ContainKey("LastRedeemDate");
            result["LastRedeemDate"].S.Should().Be(loyaltyCard.LastRedeemDate.ToString());
        }
    }
    [Fact]
    public void MapLoyaltyCardToRedeemItem_WithValidData_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var loyaltyCard = _fixture.Create<LoyaltyCard>();
        var campaignId = Guid.NewGuid();
        var rewardId = Guid.NewGuid();

        // Act
        var result = loyaltyCard.MapLoyaltyCardToRedeemItem(campaignId, rewardId);

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKeys("PK", "SK", "UserId", "BusinessId", "CampaignId", "CardId", "RewardId", "EntityType", "RedeemDate");
        
        result["PK"].S.Should().Be(Constants.UserPrefix + loyaltyCard.UserId);
        result["SK"].S.Should().StartWith(Constants.ActionRedeemBusinessPrefix + loyaltyCard.BusinessId);
        result["UserId"].S.Should().Be(loyaltyCard.UserId.ToString());
        result["BusinessId"].S.Should().Be(loyaltyCard.BusinessId.ToString());
        result["CampaignId"].S.Should().Be(campaignId.ToString());
        result["CardId"].S.Should().Be(loyaltyCard.Id.ToString());
        result["RewardId"].S.Should().Be(rewardId.ToString());
        result["EntityType"].S.Should().Be("Redeem");
        result["RedeemDate"].S.Should().Be(loyaltyCard.LastRedeemDate.ToString());
    }
    [Fact]
    public void MapLoyaltyCardToStampItem_WithValidLoyaltyCard_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var loyaltyCard = _fixture.Create<LoyaltyCard>();

        // Act
        var result = loyaltyCard.MapLoyaltyCardToStampItem();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKeys("PK", "SK", "CardId", "BusinessId", "UserId", "StampId", "EntityType", "StampDate");
        
        result["PK"].S.Should().Be(Constants.UserPrefix + loyaltyCard.UserId);
        result["SK"].S.Should().StartWith(Constants.ActionStampBusinessPrefix + loyaltyCard.BusinessId);
        
        result["CardId"].S.Should().Be(loyaltyCard.Id.ToString());
        result["BusinessId"].S.Should().Be(loyaltyCard.BusinessId.ToString());
        result["UserId"].S.Should().Be(loyaltyCard.UserId.ToString());
        result["EntityType"].S.Should().Be("Stamp");
        result["StampDate"].S.Should().Be(loyaltyCard.LastStampedDate.ToString());
    }
}

