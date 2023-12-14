using AutoFixture;
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

public static class DynamoDbMapperTests
{   
    public class UserMapping
    {
        // MapUserToItem
        [Theory, AutoData]
        public void MapUserToItem_WithValidUser_MapsAllPropertiesCorrectly(User user)
        {
            // Arrange 
            user.Status = UserStatus.Active;

            // Act
            var result = user.ToDynamoItem();

            // Assert
            result.Should().NotBeNull();
            result.Should().ContainKeys(Pk, Sk, UserId, EntityTypeAttName, Email, PhoneNumber, FirstName,
                LastName, Status);

            result[Pk].S.Should().Be(UserPrefix + user.Id);
            result[Sk].S.Should().Be(MetaUserInfo);

            result[UserId].S.Should().Be(user.Id.ToString());
            result[EntityTypeAttName].S.Should().Be(EntityType.User.ToString());
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
            var result = user.ToDynamoItem();

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
    }
    public class BusinessMapping
    {
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
            result.Should().ContainKeys(Pk, Sk, BusinessId, OwnerId, EntityTypeAttName, Name,
                OpeningHoursAttName, LocationAttributeName, Description, PhoneNumber, Email, Status);

            result[Pk].S.Should().Be(BusinessPrefix + business.Id);
            result[Sk].S.Should().Be(MetaBusinessInfo);

            result[BusinessId].S.Should().Be(business.Id.ToString());
            result[OwnerId].S.Should().Be(business.OwnerId.ToString());
            result[EntityTypeAttName].S.Should().Be(EntityType.Business.ToString());
            result[Name].S.Should().Be(business.Name);
            result[OpeningHoursAttName].S.Should().Be(JsonConvert.SerializeObject(business.OpeningHours));
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

            phoneNumber.Should().BeNull();
            description.Should().BeNull();
        }


        // MapItemToBusiness
        [Theory, AutoData]
        public void MapItemToBusiness_WithValidItem_MapsAllPropertiesCorrectly(Business business)
        {
            // Arrange
            business.Status = BusinessStatus.Active;
            var businessItem = business.CreateBusinessItem();

            // Act
            var result = businessItem.MapItemToBusiness();

            // Assert
            result.Id.Should().Be(business.Id);
            result.OwnerId.Should().Be(business.OwnerId);
            result.Name.Should().Be(business.Name);
            result.Description.Should().Be(business.Description);
            result.ContactInfo.Email.Should().Be(business.ContactInfo.Email);
            result.ContactInfo.PhoneNumber.Should().Be(business.ContactInfo.PhoneNumber);
            result.Location.Longitude.Should().Be(business.Location.Longitude);
            result.Location.Latitude.Should().Be(business.Location.Latitude);
            result.Location.Address.Should().Be(business.Location.Address);
            result.OpeningHours.Hours.Should().BeEquivalentTo(business.OpeningHours.Hours);
            result.Status.Should().Be(BusinessStatus.Active);
        }

        [Theory, AutoData]
        public void MapItemToBusiness_WithNullProperties_HandlesNullsAppropriately(Business business)
        {
            // Arrange
            business.ContactInfo.PhoneNumber = null;
            business.Description = null;

            var businessItem = business.CreateBusinessItem();

            // Act
            var result = businessItem.MapItemToBusiness();

            // Assert
            result.ContactInfo.PhoneNumber.Should().BeNull();
            result.Description.Should().BeNull();
        }

        [Theory, AutoData]
        public void MapItemToBusiness_MissingKey_ThrowsKeyNotFoundException(Business business)
        {
            // Arrange
            var businessItem = business.CreateBusinessItem();
            businessItem.Remove(Email); // Intentionally remove to test error handling

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => businessItem.MapItemToBusiness());
        }

    }
    public class BusinessUserPermissionMapping
    {
        // MapBusinessUserPermissionsToItem
        [Theory, AutoData]
        public void MapBusinessUserPermissionsToItem_WithValidPermissions_MapsAllPropertiesCorrectly(List<BusinessUserPermissions> permissions)
        {
            // Act
            var result = permissions.MapBusinessUserPermissionsToItem();

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().BeGreaterThan(0);

            foreach (var permission in permissions)
            {
                var mappedItem = result.Single(item => item[UserId].S == permission.UserId.ToString());
                mappedItem.Should().ContainKeys(Pk, Sk, UserId, BusinessId, EntityTypeAttName, Role, Timestamp, BusinessUserListPk, BusinessUserListSk);

                mappedItem[Pk].S.Should().Be(UserPrefix + permission.UserId);
                mappedItem[Sk].S.Should().Be(PermissionBusinessPrefix + permission.BusinessId);

                mappedItem[UserId].S.Should().Be(permission.UserId.ToString());
                mappedItem[BusinessId].S.Should().Be(permission.BusinessId.ToString());
                mappedItem[EntityTypeAttName].S.Should().Be(EntityType.Permission.ToString());
                mappedItem[Role].S.Should().Be(permission.Role.ToString());
                mappedItem[BusinessUserListPk].S.Should().Be(BusinessPrefix + permission.BusinessId);
                mappedItem[BusinessUserListSk].S.Should().Be(PermissionBusinessPrefix + permission.UserId);
            }
        }
        
        // MapItemToBusinessUserPermissions
        [Theory, AutoData]
        public void MapItemToBusinessUserPermissions_WithValidItem_MapsAllPropertiesCorrectly(List<BusinessUserPermissions> permissions)
        {
            // Arrange
            permissions.Select(p => p.Role = UserRole.Admin);
            var permissionItems = permissions.CreateBusinessUserPermissions();

            // Act
            var permissionsList = permissionItems.MapItemToBusinessUserPermissions();

            for (var i = 0; i < permissionItems.Count; i++)
            {
                // Assert
                /*
                permissionItems[0].BusinessId.Should().Be()
                
                result.Id.Should().Be(business.Id);
                result.OwnerId.Should().Be(business.OwnerId);
                result.Name.Should().Be(business.Name);
                result.Description.Should().Be(business.Description);
                result.ContactInfo.Email.Should().Be(business.ContactInfo.Email);
                result.ContactInfo.PhoneNumber.Should().Be(business.ContactInfo.PhoneNumber);
                result.Location.Longitude.Should().Be(business.Location.Longitude);
                result.Location.Latitude.Should().Be(business.Location.Latitude);
                result.Location.Address.Should().Be(business.Location.Address);
                result.OpeningHours.Hours.Should().BeEquivalentTo(business.OpeningHours.Hours);
                result.Status.Should().Be(BusinessStatus.Active);
                */
            }
        }

    }
    public class CampaignMapping
    {
        // Campaign
        [Fact]
        public void MapCampaignToItem_WithValidCampaign_MapsAllPropertiesCorrectly()
        {
            // Arrange
            var campaign = new Fixture().Create<Campaign>();

            // Act
            var result = campaign.MapCampaignToItem();

            // Assert
            result.Should().NotBeNull();
            result.Should().ContainKeys("PK", "SK", "BusinessId", "EntityType", "Name", "CampaignId", "Rewards",
                "StartTime", "EndTime", "IsActive");

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
    }
    public class LoyaltyCardMapping
    {
        // Loyalty Card
        [Fact]
        public void MapLoyaltyCardToItem_WithValidLoyaltyCard_MapsAllPropertiesCorrectly()
        {
            // Arrange
            var loyaltyCard = new Fixture().Create<LoyaltyCard>();

            // Act
            var result = loyaltyCard.MapLoyaltyCardToItem();

            // Assert
            result.Should().NotBeNull();
            result.Should().ContainKeys(Pk, Sk, CardId, BusinessId, UserId, EntityTypeAttName, Points, IssueDate, LastStampDate, Status, BusinessLoyaltyListPk, BusinessLoyaltyListSk);

            result[Pk].S.Should().Be(Constants.UserPrefix + loyaltyCard.UserId);
            result[Sk].S.Should().Be(Constants.CardBusinessPrefix + loyaltyCard.BusinessId);

            result[CardId].S.Should().Be(loyaltyCard.Id.ToString());
            result[BusinessId].S.Should().Be(loyaltyCard.BusinessId.ToString());
            result[UserId].S.Should().Be(loyaltyCard.UserId.ToString());
            result[EntityTypeAttName].S.Should().Be(EntityType.LoyaltyCard.ToString());
            result[Points].N.Should().Be(loyaltyCard.Points.ToString());
            result[IssueDate].S.Should().Be(loyaltyCard.IssueDate.ToString());
            result[LastStampDate].S.Should().Be(loyaltyCard.LastStampedDate.ToString());
            result[Status].S.Should().Be(loyaltyCard.Status.ToString());
            result[BusinessLoyaltyListPk].S.Should().Be(BusinessPrefix + loyaltyCard.BusinessId);
            result[BusinessLoyaltyListSk].S.Should().Be(CardUserPrefix + loyaltyCard.UserId + "#" + loyaltyCard.Id);

            if (loyaltyCard.LastUpdatedDate.HasValue)
            {
                result.Should().ContainKey(LastUpdatedDate);
                result[LastUpdatedDate].S.Should().Be(loyaltyCard.LastStampedDate.ToString());
            }

            if (loyaltyCard.LastRedeemDate.HasValue)
            {
                result.Should().ContainKey(LastRedeemDate);
                result[LastRedeemDate].S.Should().Be(loyaltyCard.LastRedeemDate.ToString());
            }
        }

        [Fact]
        public void MapLoyaltyCardToRedeemItem_WithValidData_MapsAllPropertiesCorrectly()
        {
            // Arrange
            var loyaltyCard = new Fixture().Create<LoyaltyCard>();
            var campaignId = Guid.NewGuid();
            var rewardId = Guid.NewGuid();

            // Act
            var result = loyaltyCard.MapLoyaltyCardToRedeemItem(campaignId, rewardId);

            // Assert
            result.Should().NotBeNull();
            result.Should().ContainKeys("PK", "SK", "UserId", "BusinessId", "CampaignId", "CardId", "RewardId",
                "EntityType", "RedeemDate");

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
            var loyaltyCard = new Fixture().Create<LoyaltyCard>();

            // Act
            var result = loyaltyCard.MapLoyaltyCardToStampItem();

            // Assert
            result.Should().NotBeNull();
            result.Should().ContainKeys("PK", "SK", "CardId", "BusinessId", "UserId", "StampId", "EntityType",
                "StampDate");

            result["PK"].S.Should().Be(Constants.UserPrefix + loyaltyCard.UserId);
            result["SK"].S.Should().StartWith(Constants.ActionStampBusinessPrefix + loyaltyCard.BusinessId);

            result["CardId"].S.Should().Be(loyaltyCard.Id.ToString());
            result["BusinessId"].S.Should().Be(loyaltyCard.BusinessId.ToString());
            result["UserId"].S.Should().Be(loyaltyCard.UserId.ToString());
            result["EntityType"].S.Should().Be("Stamp");
            result["StampDate"].S.Should().Be(loyaltyCard.LastStampedDate.ToString());
        }
    }
}


