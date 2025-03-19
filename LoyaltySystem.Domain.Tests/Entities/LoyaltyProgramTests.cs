using System;
using System.Linq;
using FluentAssertions;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.ValueObjects;
using Xunit;

namespace LoyaltySystem.Domain.Tests.Entities
{
    public class LoyaltyProgramTests
    {
        [Fact]
        public void Constructor_InitializesPropertiesCorrectly()
        {
            // Arrange
            var brandId = new BrandId();
            var name = "Test Program";
            var type = LoyaltyProgramType.Points;
            var description = "Test Description";
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddYears(1);
            var pointsConversionRate = 0.1m;
            
            // Act
            var program = new LoyaltyProgram(
                brandId,
                name,
                type,
                null,
                pointsConversionRate,
                null,
                false,
                null,
                null,
                null,
                description,
                null,
                0,
                null,
                startDate,
                endDate
            );
            
            // Assert
            program.Id.Should().NotBeNull();
            program.Name.Should().Be(name);
            program.Description.Should().Be(description);
            program.BrandId.Should().Be(brandId);
            program.Type.Should().Be(type);
            program.PointsConversionRate.Should().Be(pointsConversionRate);
            program.StartDate.Should().Be(startDate);
            program.EndDate.Should().Be(endDate);
            program.IsActive.Should().BeTrue();
            program.Tiers.Should().NotBeNull();
            program.Tiers.Should().BeEmpty();
            program.Rewards.Should().NotBeNull();
            program.Rewards.Should().BeEmpty();
        }
        
        [Fact]
        public void Update_ChangesProgramInformation()
        {
            // Arrange
            var brandId = new BrandId();
            var program = new LoyaltyProgram(
                brandId,
                "Original Name",
                LoyaltyProgramType.Points,
                null,
                0.1m,
                null,
                false,
                null,
                null,
                null,
                "Original Description",
                null,
                0,
                null,
                DateTime.UtcNow,
                DateTime.UtcNow.AddYears(1)
            );
            
            var newName = "Updated Name";
            var newDescription = "Updated Description";
            var newStartDate = DateTime.UtcNow.AddMonths(1);
            var newEndDate = newStartDate.AddYears(2);
            
            // Act
            program.Update(
                newName,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                newDescription,
                null,
                null,
                newStartDate,
                newEndDate
            );
            
            // Assert
            program.Name.Should().Be(newName);
            program.Description.Should().Be(newDescription);
            program.StartDate.Should().Be(newStartDate);
            program.EndDate.Should().Be(newEndDate);
        }
        
        [Fact]
        public void Deactivate_SetsIsActiveToFalse()
        {
            // Arrange
            var brandId = new BrandId();
            var program = new LoyaltyProgram(
                brandId,
                "Test Program",
                LoyaltyProgramType.Points,
                null,
                0.1m,
                null,
                false,
                null,
                null,
                null,
                "Test Description",
                null,
                0,
                null,
                DateTime.UtcNow,
                DateTime.UtcNow.AddYears(1)
            );
            
            // Act
            program.Deactivate();
            
            // Assert
            program.IsActive.Should().BeFalse();
        }
        
        [Fact]
        public void Activate_SetsIsActiveToTrue()
        {
            // Arrange
            var brandId = new BrandId();
            var program = new LoyaltyProgram(
                brandId,
                "Test Program",
                LoyaltyProgramType.Points,
                null,
                0.1m,
                null,
                false,
                null,
                null,
                null,
                "Test Description",
                null,
                0,
                null,
                DateTime.UtcNow,
                DateTime.UtcNow.AddYears(1)
            );
            program.Deactivate(); // First deactivate to ensure it's inactive
            
            // Act
            program.Activate();
            
            // Assert
            program.IsActive.Should().BeTrue();
        }
        
        [Fact]
        public void CreateTier_AddsTierToProgram()
        {
            // Arrange
            var brandId = new BrandId();
            var program = new LoyaltyProgram(
                brandId,
                "Test Program",
                LoyaltyProgramType.Points,
                null,
                0.1m,
                null,
                true, // Has tiers enabled
                null,
                null,
                null,
                "Test Description",
                null,
                0,
                null,
                DateTime.UtcNow,
                DateTime.UtcNow.AddYears(1)
            );
            
            var tierName = "Gold";
            var pointThreshold = 1000;
            var pointMultiplier = 2.0m;
            var tierOrder = 2;
            
            // Act
            var tier = program.CreateTier(
                tierName,
                pointThreshold,
                pointMultiplier,
                tierOrder
            );
            
            // Assert
            program.Tiers.Should().NotBeEmpty();
            program.Tiers.Should().HaveCount(1);
            program.Tiers.First().Name.Should().Be(tierName);
            program.Tiers.First().PointThreshold.Should().Be(pointThreshold);
            program.Tiers.First().PointMultiplier.Should().Be(pointMultiplier);
            program.Tiers.First().TierOrder.Should().Be(tierOrder);
        }
        
        [Fact]
        public void AddTier_AddsTierToProgram()
        {
            // Arrange
            var brandId = new BrandId();
            var programId = new LoyaltyProgramId();
            var program = new LoyaltyProgram(
                brandId,
                "Test Program",
                LoyaltyProgramType.Points,
                null,
                0.1m,
                null,
                true, // Has tiers enabled
                null,
                null,
                null,
                "Test Description",
                null,
                0,
                programId,
                DateTime.UtcNow,
                DateTime.UtcNow.AddYears(1)
            );
            
            var tier = new LoyaltyTier(
                programId,
                "Gold",
                1000,
                2.0m,
                2
            );
            
            // Act
            program.AddTier(tier);
            
            // Assert
            program.Tiers.Should().NotBeEmpty();
            program.Tiers.Should().HaveCount(1);
            program.Tiers.Should().Contain(tier);
        }
        
        [Fact]
        public void CreateReward_AddsRewardToProgram()
        {
            // Arrange
            var brandId = new BrandId();
            var program = new LoyaltyProgram(
                brandId,
                "Test Program",
                LoyaltyProgramType.Points,
                null,
                0.1m,
                null,
                false,
                null,
                null,
                null,
                "Test Description",
                null,
                0,
                null,
                DateTime.UtcNow,
                DateTime.UtcNow.AddYears(1)
            );
            
            var title = "Free Coffee";
            var description = "Get a free coffee";
            var requiredValue = 100;
            var validFrom = DateTime.UtcNow;
            var validTo = DateTime.UtcNow.AddMonths(3);
            
            // Act
            var reward = program.CreateReward(
                title,
                description,
                requiredValue,
                validFrom,
                validTo
            );
            
            // Assert
            program.Rewards.Should().NotBeEmpty();
            program.Rewards.Should().HaveCount(1);
            program.Rewards.First().Title.Should().Be(title);
            program.Rewards.First().Description.Should().Be(description);
            program.Rewards.First().RequiredValue.Should().Be(requiredValue);
            program.Rewards.First().ValidFrom.Should().Be(validFrom);
            program.Rewards.First().ValidTo.Should().Be(validTo);
        }
        
        [Fact]
        public void AddReward_AddsRewardToProgram()
        {
            // Arrange
            var brandId = new BrandId();
            var programId = new LoyaltyProgramId();
            var program = new LoyaltyProgram(
                brandId,
                "Test Program",
                LoyaltyProgramType.Points,
                null,
                0.1m,
                null,
                false,
                null,
                null,
                null,
                "Test Description",
                null,
                0,
                programId,
                DateTime.UtcNow,
                DateTime.UtcNow.AddYears(1)
            );
            
            var reward = new Reward(
                programId,
                "Free Coffee",
                "Get a free coffee",
                100,
                DateTime.UtcNow,
                DateTime.UtcNow.AddMonths(3)
            );
            
            // Act
            program.AddReward(reward);
            
            // Assert
            program.Rewards.Should().NotBeEmpty();
            program.Rewards.Should().HaveCount(1);
            program.Rewards.Should().Contain(reward);
        }
        
        [Fact]
        public void GetTierForPoints_ReturnsCorrectTier()
        {
            // Arrange
            var brandId = new BrandId();
            var programId = new LoyaltyProgramId();
            var program = new LoyaltyProgram(
                brandId,
                "Test Program",
                LoyaltyProgramType.Points,
                null,
                0.1m,
                null,
                true, // Has tiers enabled
                null,
                null,
                null,
                "Test Description",
                null,
                0,
                programId,
                DateTime.UtcNow,
                DateTime.UtcNow.AddYears(1)
            );
            
            var bronzeTier = new LoyaltyTier(
                programId,
                "Bronze",
                0,
                1.0m,
                0
            );
            
            var silverTier = new LoyaltyTier(
                programId,
                "Silver",
                500,
                1.5m,
                1
            );
            
            var goldTier = new LoyaltyTier(
                programId,
                "Gold",
                1000,
                2.0m,
                2
            );
            
            var platinumTier = new LoyaltyTier(
                programId,
                "Platinum",
                2000,
                3.0m,
                3
            );
            
            program.AddTier(bronzeTier);
            program.AddTier(silverTier);
            program.AddTier(goldTier);
            program.AddTier(platinumTier);
            
            // Act & Assert
            program.GetTierForPoints(0).Should().Be(bronzeTier);
            program.GetTierForPoints(100).Should().Be(bronzeTier);
            program.GetTierForPoints(500).Should().Be(silverTier);
            program.GetTierForPoints(750).Should().Be(silverTier);
            program.GetTierForPoints(1000).Should().Be(goldTier);
            program.GetTierForPoints(1500).Should().Be(goldTier);
            program.GetTierForPoints(2000).Should().Be(platinumTier);
            program.GetTierForPoints(3000).Should().Be(platinumTier);
        }
        
        [Fact]
        public void IsValidForPointsIssuance_ReturnsFalse_WhenProgramIsNotActive()
        {
            // Arrange
            var brandId = new BrandId();
            var program = new LoyaltyProgram(
                brandId,
                "Test Program",
                LoyaltyProgramType.Points,
                null,
                0.1m,
                null,
                false,
                null,
                null,
                null,
                "Test Description",
                null,
                0,
                null,
                DateTime.UtcNow,
                DateTime.UtcNow.AddYears(1)
            );
            program.Deactivate();
            
            // Act & Assert
            program.IsValidForPointsIssuance(100).Should().BeFalse();
        }
        
        [Fact]
        public void CalculatePoints_CalculatesCorrectly_ForBasicRate()
        {
            // Arrange
            var brandId = new BrandId();
            var program = new LoyaltyProgram(
                brandId,
                "Test Program",
                LoyaltyProgramType.Points,
                null,
                0.1m, // 1 point per $10 spent
                null,
                false,
                null,
                null,
                null,
                "Test Description",
                null,
                0,
                null,
                DateTime.UtcNow,
                DateTime.UtcNow.AddYears(1)
            );
            
            // Act & Assert
            program.CalculatePoints(100).Should().Be(10); // $100 * 0.1 = 10 points
            program.CalculatePoints(25).Should().Be(2);   // $25 * 0.1 = 2.5, floor to 2 points
            program.CalculatePoints(5).Should().Be(0);    // $5 * 0.1 = 0.5, floor to 0 points
        }
    }
} 