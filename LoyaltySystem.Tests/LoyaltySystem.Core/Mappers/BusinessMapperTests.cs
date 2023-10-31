using AutoFixture;
using FluentAssertions;
using LoyaltySystem.Core.DTOs;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Mappers;
using Xunit;

namespace Tests.LoyaltySystem.Core.Mappers;

public class BusinessMapperTests
{
    [Fact]
    public void CreateBusinessFromCreateBusinessDto_WithValidDto_ShouldReturnBusiness()
    {
        // Arrange
        var createBusinessDto = new Fixture().Build<CreateBusinessDto>().Create();
        
        // Act
        var converted = new BusinessMapper().CreateBusinessFromCreateBusinessDto(createBusinessDto);
        
        // Assert
        converted.OwnerId.Should().Be(createBusinessDto.OwnerId);
        converted.Name.Should().Be(createBusinessDto.Name);
        converted.Description.Should().Be(createBusinessDto.Description);
        converted.Location.Should().Be(createBusinessDto.Location);
        converted.ContactInfo.Should().Be(createBusinessDto.ContactInfo);
        converted.OpeningHours.Should().Be(createBusinessDto.OpeningHours);
        converted.Status.Should().Be(BusinessStatus.Pending);
    }
}