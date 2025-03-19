using System;
using FluentAssertions;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.ValueObjects;
using Xunit;

namespace LoyaltySystem.Domain.Tests.Entities
{
    public class CustomerTests
    {
        [Fact]
        public void Constructor_InitializesPropertiesCorrectly()
        {
            // Arrange
            var firstName = "John";
            var lastName = "Doe";
            var username = "johndoe";
            var email = "john.doe@example.com";
            var phone = "1234567890";
            var address = new Address("123 Main St", "Apt 4B", "New York", "NY", "10001", "US");
            var marketingConsent = true;
            
            // Act
            var customer = new Customer(
                null,
                firstName,
                lastName,
                username,
                email,
                phone,
                address,
                marketingConsent
            );
            
            // Assert
            customer.Id.Should().NotBeNull();
            customer.FirstName.Should().Be(firstName);
            customer.LastName.Should().Be(lastName);
            customer.UserName.Should().Be(username);
            customer.Email.Should().Be(email);
            customer.Phone.Should().Be(phone);
            customer.Address.Should().Be(address);
            customer.MarketingConsent.Should().Be(marketingConsent);
            customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            customer.LoyaltyCards.Should().NotBeNull();
            customer.LoyaltyCards.Should().BeEmpty();
        }
        
        [Fact]
        public void Update_ChangesCustomerInformation()
        {
            // Arrange
            var customer = new Customer(
                null,
                "John",
                "Doe",
                "johndoe",
                "john.doe@example.com",
                "1234567890",
                null,
                true
            );
            
            var newFirstName = "Jane";
            var newLastName = "Smith";
            var newUsername = "janesmith";
            var newEmail = "jane.smith@example.com";
            var newPhone = "0987654321";
            var newAddress = new Address("456 Oak St", null, "Boston", "MA", "02108", "US");
            var newMarketingConsent = false;
            
            // Act
            customer.Update(
                newFirstName,
                newLastName,
                newUsername,
                newEmail,
                newPhone,
                newAddress,
                newMarketingConsent
            );
            
            // Assert
            customer.FirstName.Should().Be(newFirstName);
            customer.LastName.Should().Be(newLastName);
            customer.UserName.Should().Be(newUsername);
            customer.Email.Should().Be(newEmail);
            customer.Phone.Should().Be(newPhone);
            customer.Address.Should().Be(newAddress);
            customer.MarketingConsent.Should().Be(newMarketingConsent);
            customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
        
        [Fact]
        public void RecordLogin_UpdatesLastLoginAt()
        {
            // Arrange
            var customer = new Customer(
                null,
                "John",
                "Doe",
                "johndoe",
                "john.doe@example.com",
                "1234567890",
                null,
                true
            );
            
            // Act
            customer.RecordLogin();
            
            // Assert
            customer.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
        
        [Fact]
        public void AddLoyaltyCard_AddsCardToCustomer()
        {
            // Arrange
            var customer = new Customer(
                null,
                "John",
                "Doe",
                "johndoe",
                "john.doe@example.com",
                "1234567890",
                null,
                true
            );
            
            var programId = new LoyaltyProgramId();
            var customerId = customer.Id;
            var loyaltyCard = new LoyaltyCard(
                programId,
                customerId,
                LoyaltySystem.Domain.Enums.LoyaltyProgramType.Points
            );
            
            // Act
            customer.AddLoyaltyCard(loyaltyCard);
            
            // Assert
            customer.LoyaltyCards.Should().NotBeEmpty();
            customer.LoyaltyCards.Should().HaveCount(1);
            customer.LoyaltyCards.Should().Contain(loyaltyCard);
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Constructor_ThrowsArgumentException_WhenFirstNameIsInvalid(string invalidFirstName)
        {
            // Arrange & Act & Assert
            Action action = () => new Customer(
                null,
                invalidFirstName,
                "Doe",
                "johndoe",
                "john.doe@example.com",
                "1234567890",
                null
            );
            
            action.Should().Throw<ArgumentException>()
                .WithMessage("FirstName cannot be empty (Parameter 'firstName')");
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Constructor_ThrowsArgumentException_WhenLastNameIsInvalid(string invalidLastName)
        {
            // Arrange & Act & Assert
            Action action = () => new Customer(
                null,
                "John",
                invalidLastName,
                "johndoe",
                "john.doe@example.com",
                "1234567890",
                null
            );
            
            action.Should().Throw<ArgumentException>()
                .WithMessage("LastName cannot be empty (Parameter 'lastName')");
        }
        
        [Theory]
        [InlineData("invalidemail")]
        [InlineData("invalid@")]
        public void Constructor_ThrowsArgumentException_WhenEmailIsInvalid(string invalidEmail)
        {
            // Arrange & Act & Assert
            Action action = () => new Customer(null, "John", "Doe", "johndoe", invalidEmail, "1234567890", null);
            
            action.Should().Throw<ArgumentException>()
                .WithMessage("Invalid email format (Parameter 'email')");
        }
    }
} 