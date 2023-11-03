using AutoFixture;
using FluentAssertions;
using LoyaltySystem.Core.Dtos;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Mappers;
using Xunit;

namespace Tests.LoyaltySystem.Core.Mappers;

public class UserMapperTests
{
    [Fact]
    public void CreateUserFromCreateUserDto_WithValidDto_ShouldReturnUser()
    {
        // Arrange
        var createUserDto = new Fixture().Build<CreateUserDto>().Create();
        
        // Act
        var converted = new UserMapper().CreateUserFromCreateUserDto(createUserDto);
        
        // Assert
        converted.FirstName.Should().Be(createUserDto.FirstName);
        converted.LastName.Should().Be(createUserDto.LastName);
        converted.ContactInfo.Should().Be(createUserDto.ContactInfo);
        converted.DateOfBirth.Should().Be(createUserDto.DateOfBirth);
        converted.Status.Should().Be(UserStatus.Pending);
    }
}