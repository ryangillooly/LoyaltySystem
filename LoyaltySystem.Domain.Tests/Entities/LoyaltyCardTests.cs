using System;
using FluentAssertions;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Common;
using Xunit;

namespace LoyaltySystem.Domain.Tests.Entities
{
    public class LoyaltyCardTests
    {
        [Fact]
        public void Constructor_InitializesPropertiesCorrectly()
        {
            // Arrange
            var programId = new LoyaltyProgramId();
            var customerId = new CustomerId();
            var type = LoyaltyProgramType.Points;
            
            // Act
            var loyaltyCard = new LoyaltyCard(
                programId,
                customerId,
                type
            );
            
            // Assert
            loyaltyCard.Id.Should().NotBeNull();
            loyaltyCard.ProgramId.Should().Be(programId);
            loyaltyCard.CustomerId.Should().Be(customerId);
            loyaltyCard.Type.Should().Be(type);
            loyaltyCard.StampsCollected.Should().Be(0);
            loyaltyCard.PointsBalance.Should().Be(0);
            loyaltyCard.Status.Should().Be(CardStatus.Active);
            loyaltyCard.QrCode.Should().NotBeNullOrEmpty();
            loyaltyCard.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            loyaltyCard.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            loyaltyCard.Transactions.Should().NotBeNull();
            loyaltyCard.Transactions.Should().BeEmpty();
        }
        
        [Fact]
        public void AddPoints_IncreasesPointsBalance()
        {
            // Arrange
            var programId = new LoyaltyProgramId();
            var customerId = new CustomerId();
            var loyaltyCard = new LoyaltyCard(
                programId,
                customerId,
                LoyaltyProgramType.Points
            );
            
            var pointsToAdd = 50m;
            var transactionAmount = 100m;
            var storeId = new StoreId();
            
            // Act
            var transaction = loyaltyCard.AddPoints(pointsToAdd, transactionAmount, storeId);
            
            // Assert
            loyaltyCard.PointsBalance.Should().Be(pointsToAdd);
            transaction.Should().NotBeNull();
            transaction.CardId.Should().Be(loyaltyCard.Id);
            transaction.Type.Should().Be(TransactionType.PointsIssuance);
            transaction.PointsAmount.Should().Be(pointsToAdd);
            transaction.TransactionAmount.Should().Be(transactionAmount);
        }
        
        [Fact]
        public void IssueStamps_IncreasesStampsCollected()
        {
            // Arrange
            var programId = new LoyaltyProgramId();
            var customerId = new CustomerId();
            var loyaltyCard = new LoyaltyCard(
                programId,
                customerId,
                LoyaltyProgramType.Stamp
            );
            
            var stampsToAdd = 3;
            var storeId = Guid.NewGuid();
            
            // Act
            var transaction = loyaltyCard.IssueStamps(stampsToAdd, storeId);
            
            // Assert
            loyaltyCard.StampsCollected.Should().Be(stampsToAdd);
            transaction.Should().NotBeNull();
            transaction.CardId.Should().Be(loyaltyCard.Id);
            transaction.Type.Should().Be(TransactionType.StampIssuance);
            transaction.Quantity.Should().Be(stampsToAdd);
        }
        
        [Fact]
        public void IssueStamps_ThrowsException_WhenTypeIsPoints()
        {
            // Arrange
            var programId = new LoyaltyProgramId();
            var customerId = new CustomerId();
            var loyaltyCard = new LoyaltyCard(
                programId,
                customerId,
                LoyaltyProgramType.Points
            );
            
            var stampsToAdd = 3;
            var storeId = Guid.NewGuid();
            
            // Act & Assert
            Action action = () => loyaltyCard.IssueStamps(stampsToAdd, storeId);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Cannot issue stamps to a points-based card");
        }
        
        [Fact]
        public void AddPoints_ThrowsException_WhenTypeIsStamp()
        {
            // Arrange
            var programId = new LoyaltyProgramId();
            var customerId = new CustomerId();
            var loyaltyCard = new LoyaltyCard(
                programId,
                customerId,
                LoyaltyProgramType.Stamp
            );
            
            var pointsToAdd = 50m;
            var transactionAmount = 100m;
            var storeId = new StoreId();
            
            // Act & Assert
            Action action = () => loyaltyCard.AddPoints(pointsToAdd, transactionAmount, storeId);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Cannot add points to a stamp-based card");
        }
        
        [Fact]
        public void Expire_SetsStatusToExpired()
        {
            // Arrange
            var programId = new LoyaltyProgramId();
            var customerId = new CustomerId();
            var loyaltyCard = new LoyaltyCard(
                programId,
                customerId,
                LoyaltyProgramType.Points
            );
            
            // Act
            loyaltyCard.Expire();
            
            // Assert
            loyaltyCard.Status.Should().Be(CardStatus.Expired);
        }
        
        [Fact]
        public void Suspend_SetsStatusToSuspended()
        {
            // Arrange
            var programId = new LoyaltyProgramId();
            var customerId = new CustomerId();
            var loyaltyCard = new LoyaltyCard(
                programId,
                customerId,
                LoyaltyProgramType.Points
            );
            
            // Act
            loyaltyCard.Suspend();
            
            // Assert
            loyaltyCard.Status.Should().Be(CardStatus.Suspended);
        }
        
        [Fact]
        public void Reactivate_SetsStatusToActive_WhenSuspended()
        {
            // Arrange
            var programId = new LoyaltyProgramId();
            var customerId = new CustomerId();
            var loyaltyCard = new LoyaltyCard(
                programId,
                customerId,
                LoyaltyProgramType.Points
            );
            loyaltyCard.Suspend(); // First suspend
            
            // Act
            loyaltyCard.Reactivate();
            
            // Assert
            loyaltyCard.Status.Should().Be(CardStatus.Active);
        }
        
        [Fact]
        public void Reactivate_ThrowsException_WhenNotSuspended()
        {
            // Arrange
            var programId = new LoyaltyProgramId();
            var customerId = new CustomerId();
            var loyaltyCard = new LoyaltyCard(
                programId,
                customerId,
                LoyaltyProgramType.Points
            );
            // Card is Active by default
            
            // Act & Assert
            Action action = () => loyaltyCard.Reactivate();
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Can only reactivate suspended cards");
        }
        
        [Fact]
        public void SetExpirationDate_SetsExpiresAt()
        {
            // Arrange
            var programId = new LoyaltyProgramId();
            var customerId = new CustomerId();
            var loyaltyCard = new LoyaltyCard(
                programId,
                customerId,
                LoyaltyProgramType.Points
            );
            
            var expirationDate = DateTime.UtcNow.AddYears(1);
            
            // Act
            loyaltyCard.SetExpirationDate(expirationDate);
            
            // Assert
            loyaltyCard.ExpiresAt.Should().Be(expirationDate);
        }
        
        [Fact]
        public void SetExpirationDate_ThrowsException_WhenDateIsInPast()
        {
            // Arrange
            var programId = new LoyaltyProgramId();
            var customerId = new CustomerId();
            var loyaltyCard = new LoyaltyCard(
                programId,
                customerId,
                LoyaltyProgramType.Points
            );
            
            var pastDate = DateTime.UtcNow.AddDays(-1);
            
            // Act & Assert
            Action action = () => loyaltyCard.SetExpirationDate(pastDate);
            action.Should().Throw<ArgumentException>()
                .WithMessage("Expiration date must be in the future (Parameter 'expirationDate')");
        }
        
        [Fact]
        public void GetStampsIssuedToday_ReturnsCorrectCount()
        {
            // Arrange
            var programId = new LoyaltyProgramId();
            var customerId = new CustomerId();
            var loyaltyCard = new LoyaltyCard(
                programId,
                customerId,
                LoyaltyProgramType.Stamp
            );
            
            var storeId = Guid.NewGuid();
            loyaltyCard.IssueStamps(2, storeId);
            loyaltyCard.IssueStamps(3, storeId);
            
            // Act
            var stampsToday = loyaltyCard.GetStampsIssuedToday();
            
            // Assert
            stampsToday.Should().Be(5);
        }
    }
} 