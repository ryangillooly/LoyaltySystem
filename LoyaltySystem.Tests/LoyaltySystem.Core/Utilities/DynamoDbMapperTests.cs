using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using static LoyaltySystem.Core.Models.Constants;
using LoyaltySystem.Core.Utilities;
using Newtonsoft.Json;
using Xunit;

namespace Tests.LoyaltySystem.Core.Utilities;

public class DynamoDbMapperTests
{
    private readonly IFixture        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    private readonly IDynamoDbMapper _mapper  = new DynamoDbMapper();

    // User
    [Fact]
    public void MapUserToItem_WithValidUser_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var user = _fixture.Build<User>()
                           .With(r => r.DateOfBirth, new DateTime(1980, 1, 1))
                           .Create();

        // Act
        var result = _mapper.MapUserToItem(user);

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKeys("PK", "SK", "UserId", "EntityType", "Email", "PhoneNumber", "FirstName", "LastName", "Status");
        
        result["PK"].S.Should().Be(Constants.UserPrefix + user.Id);
        result["SK"].S.Should().Be(Constants.MetaUserInfo);
        
        result["UserId"].S.Should().Be(user.Id.ToString());
        result["EntityType"].S.Should().Be(EntityType.User.ToString());
        result["Email"].S.Should().Be(user.ContactInfo.Email);
        result["PhoneNumber"].S.Should().Be(user.ContactInfo.PhoneNumber);
        result["FirstName"].S.Should().Be(user.FirstName);
        result["LastName"].S.Should().Be(user.LastName);
        result["Status"].S.Should().Be(user.Status.ToString());
        
        if (user.DateOfBirth.HasValue) 
            result["DateOfBirth"].S.Should().Be(user.DateOfBirth.Value.ToString("yyyy-MM-dd"));
    }
    
    // Business
    [Fact]
    public void MapBusinessToItem_WithValidBusiness_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var business = _fixture.Create<Business>();

        // Act
        var result = _mapper.MapBusinessToItem(business);

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKeys("PK", "SK", "BusinessId", "OwnerId", "EntityType", "Name", "OpeningHours", "Location", "Desc", "PhoneNumber", "Email", "Status");
        
        result["PK"].S.Should().Be(BusinessPrefix + business.Id);
        result["SK"].S.Should().Be(MetaBusinessInfo);
        result["BusinessId"].S.Should().Be(business.Id.ToString());
        result["OwnerId"].S.Should().Be(business.OwnerId.ToString());
        result["EntityType"].S.Should().Be(EntityType.Business.ToString());
        result["Name"].S.Should().Be(business.Name);
        result["OpeningHours"].S.Should().Be(JsonConvert.SerializeObject(business.OpeningHours));
        result["Location"].S.Should().Be(JsonConvert.SerializeObject(business.Location));
        result["Desc"].S.Should().Be(business.Description);
        result["PhoneNumber"].S.Should().Be(business.ContactInfo.PhoneNumber);
        result["Email"].S.Should().Be(business.ContactInfo.Email);
        result["Status"].S.Should().Be(business.Status.ToString());
    }
    [Fact]
    public void MapBusinessUserPermissionsToItem_WithValidPermissions_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var permissions = _fixture.CreateMany<BusinessUserPermissions>(5).ToList();

        // Act
        var result = _mapper.MapBusinessUserPermissionsToItem(permissions);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        foreach (var permission in permissions)
        {
            var mappedItem = result.Single(item => item["UserId"].S == permission.UserId.ToString());
            mappedItem.Should().ContainKeys("PK", "SK", "UserId", "BusinessId", "EntityType", "Role", "Timestamp", "BusinessUserList-PK", "BusinessUserList-SK");
           
            mappedItem["PK"].S.Should().Be(UserPrefix + permission.UserId);
            mappedItem["SK"].S.Should().Be(PermissionBusinessPrefix + permission.BusinessId);
            mappedItem["UserId"].S.Should().Be(permission.UserId.ToString());
            mappedItem["BusinessId"].S.Should().Be(permission.BusinessId.ToString());
            mappedItem["EntityType"].S.Should().Be(EntityType.Permission.ToString());
            mappedItem["Role"].S.Should().Be(permission.Role.ToString());
            mappedItem["BusinessUserList-PK"].S.Should().Be(permission.BusinessId.ToString());
            mappedItem["BusinessUserList-SK"].S.Should().Be(PermissionBusinessPrefix + permission.UserId);
        }
    }
    
    // Campaign
    [Fact]
    public void MapCampaignToItem_WithValidCampaign_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var campaign = _fixture.Create<Campaign>();

        // Act
        var result = _mapper.MapCampaignToItem(campaign);

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
        var result = _mapper.MapLoyaltyCardToItem(loyaltyCard);

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
        var result = _mapper.MapLoyaltyCardToRedeemItem(loyaltyCard, campaignId, rewardId);

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
        var result = _mapper.MapLoyaltyCardToStampItem(loyaltyCard);

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


