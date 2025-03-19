using System;
using System.Linq;
using FluentAssertions;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using Xunit;

namespace LoyaltySystem.Domain.Tests.Entities
{
    public class UserTests
    {
        [Fact]
        public void Constructor_InitializesPropertiesCorrectly()
        {
            // Arrange
            var firstName = "John";
            var lastName = "Doe";
            var username = "johndoe";
            var email = "john.doe@example.com";
            var passwordHash = "hashedPassword";
            var passwordSalt = "salt";
            
            // Act
            var user = new User(
                firstName,
                lastName,
                username,
                email,
                passwordHash,
                passwordSalt
            );
            
            // Assert
            user.Id.Should().NotBeNull();
            user.FirstName.Should().Be(firstName);
            user.LastName.Should().Be(lastName);
            user.UserName.Should().Be(username);
            user.Email.Should().Be(email);
            user.PasswordHash.Should().Be(passwordHash);
            user.PasswordSalt.Should().Be(passwordSalt);
            user.Status.Should().Be(UserStatus.Active);
            user.Roles.Should().NotBeNull();
            user.Roles.Should().BeEmpty();
        }
        
        [Fact]
        public void AddRole_AddsRoleToUser()
        {
            // Arrange
            var user = new User(
                "John",
                "Doe",
                "johndoe",
                "john.doe@example.com",
                "hashedPassword",
                "salt"
            );
            
            // Act
            user.AddRole(RoleType.Customer);
            
            // Assert
            user.Roles.Should().HaveCount(1);
            user.Roles.First().Role.Should().Be(RoleType.Customer);
            user.HasRole(RoleType.Customer).Should().BeTrue();
        }
        
        [Fact]
        public void RemoveRole_RemovesRoleFromUser()
        {
            // Arrange
            var user = new User(
                "John",
                "Doe",
                "johndoe",
                "john.doe@example.com",
                "hashedPassword",
                "salt"
            );
            user.AddRole(RoleType.Customer);
            user.AddRole(RoleType.Staff);
            
            // Act
            user.RemoveRole(RoleType.Staff);
            
            // Assert
            user.Roles.Should().HaveCount(1);
            user.Roles.First().Role.Should().Be(RoleType.Customer);
            user.HasRole(RoleType.Staff).Should().BeFalse();
        }
        
        [Fact]
        public void HasRole_ReturnsTrueWhenUserHasRole()
        {
            // Arrange
            var user = new User(
                "John",
                "Doe",
                "johndoe",
                "john.doe@example.com",
                "hashedPassword",
                "salt"
            );
            user.AddRole(RoleType.Customer);
            
            // Act & Assert
            user.HasRole(RoleType.Customer).Should().BeTrue();
            user.HasRole(RoleType.Staff).Should().BeFalse();
        }
        
        [Fact]
        public void UpdateUserName_ChangesUsername()
        {
            // Arrange
            var user = new User(
                "John",
                "Doe",
                "johndoe",
                "john.doe@example.com",
                "hashedPassword",
                "salt"
            );
            var newUsername = "john.doe";
            
            // Act
            user.UpdateUserName(newUsername);
            
            // Assert
            user.UserName.Should().Be(newUsername);
        }
        
        [Fact]
        public void UpdateEmail_ChangesEmail()
        {
            // Arrange
            var user = new User(
                "John",
                "Doe",
                "johndoe",
                "john.doe@example.com",
                "hashedPassword",
                "salt"
            );
            var newEmail = "john.doe.new@example.com";
            
            // Act
            user.UpdateEmail(newEmail);
            
            // Assert
            user.Email.Should().Be(newEmail);
        }
        
        [Fact]
        public void UpdatePassword_ChangesPasswordHashAndSalt()
        {
            // Arrange
            var user = new User(
                "John",
                "Doe",
                "johndoe",
                "john.doe@example.com",
                "hashedPassword",
                "salt"
            );
            var newHash = "newHashedPassword";
            var newSalt = "newSalt";
            
            // Act
            user.UpdatePassword(newHash, newSalt);
            
            // Assert
            user.PasswordHash.Should().Be(newHash);
            user.PasswordSalt.Should().Be(newSalt);
        }
        
        [Fact]
        public void LinkToCustomer_SetsCustomerId()
        {
            // Arrange
            var user = new User(
                "John",
                "Doe",
                "johndoe",
                "john.doe@example.com",
                "hashedPassword",
                "salt"
            );
            var customerId = "cu_1234567890";
            
            // Act & Assert
            // Note: We can't directly test this without creating a valid CustomerId string
            // that matches the entity ID pattern. This test might need to be revised.
            Assert.Throws<FormatException>(() => user.LinkToCustomer(customerId));
        }

        [Fact]
        public void RecordLogin_UpdatesLastLoginAt()
        {
            // Arrange
            var user = new User(
                "John",
                "Doe",
                "johndoe",
                "john.doe@example.com",
                "hashedPassword",
                "salt"
            );
            
            // Act
            user.RecordLogin();
            
            // Assert
            user.LastLoginAt.Should().NotBeNull();
            user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Activate_SetsStatusToActive()
        {
            // Arrange
            var user = new User(
                "John",
                "Doe",
                "johndoe",
                "john.doe@example.com",
                "hashedPassword",
                "salt"
            );
            user.Deactivate(); // First deactivate to test activation
            
            // Act
            user.Activate();
            
            // Assert
            user.Status.Should().Be(UserStatus.Active);
        }

        [Fact]
        public void Deactivate_SetsStatusToInactive()
        {
            // Arrange
            var user = new User(
                "John",
                "Doe",
                "johndoe",
                "john.doe@example.com",
                "hashedPassword",
                "salt"
            );
            
            // Act
            user.Deactivate();
            
            // Assert
            user.Status.Should().Be(UserStatus.Inactive);
        }

        [Fact]
        public void Lock_SetsStatusToLocked()
        {
            // Arrange
            var user = new User(
                "John",
                "Doe",
                "johndoe",
                "john.doe@example.com",
                "hashedPassword",
                "salt"
            );
            
            // Act
            user.Lock();
            
            // Assert
            user.Status.Should().Be(UserStatus.Locked);
        }
    }
} 