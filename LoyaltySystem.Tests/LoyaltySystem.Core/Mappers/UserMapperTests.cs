using AutoFixture;
using AutoMapper;
using FluentAssertions;
using LoyaltySystem.Core.Dtos;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Mappers;
using LoyaltySystem.Core.MappingProfiles;
using LoyaltySystem.Core.Models;
using Xunit;

namespace Tests.LoyaltySystem.Core.Mappers;

public class UserMapperTests
{
    private readonly IMapper _mapper;

    public UserMapperTests()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddProfile<UserProfile>(); });
        _mapper = config.CreateMapper();
    }
    
    [Fact]
    public void CreateUserFromCreateUserDto_WithValidDto_ShouldReturnUser()
    {
        // Arrange
        var createUserDto = new Fixture().Build<CreateUserDto>().With(r => r.DateOfBirth, new DateOnly(1995,04,20)).Create();
        
        // Act
        var converted = _mapper.Map<User>(createUserDto);
        
        // Assert
        converted.FirstName.Should().Be(createUserDto.FirstName);
        converted.LastName.Should().Be(createUserDto.LastName);
        converted.ContactInfo.Should().Be(createUserDto.ContactInfo);
        converted.DateOfBirth.Should().Be(createUserDto.DateOfBirth);
        converted.Status.Should().Be(UserStatus.Pending);
    }
}