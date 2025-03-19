using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Mvc;
using LoyaltySystem.Customer.API.Controllers;
using LoyaltySystem.Customer.API.Dtos;
using LoyaltySystem.Customer.API.Services;

namespace LoyaltySystem.Customer.API.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly AuthController _controller;
        private readonly Mock<IAuthService> _authServiceMock;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _controller = new AuthController(_authServiceMock.Object);
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var userDto = new UserDto
            {
                Id = "123",
                FirstName = "Test",
                LastName = "User",
                UserName = "testuser",
                Email = "test@example.com",
                Phone = "1234567890",
                Status = "Active",
                Roles = new List<string> { "Administrator" }
            };

            var registerDto = new RegisterUserDto
            {
                FirstName = "Test",
                LastName = "User",
                UserName = "testuser",
                Email = "test@example.com",
                Phone = "1234567890",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            _authServiceMock.Setup(x => x.RegisterUserAsync(It.IsAny<RegisterUserDto>()))
                .ReturnsAsync(OperationResult<UserDto>.SuccessResult(userDto));

            // Act
            var result = await _controller.RegisterUser(registerDto);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var apiResponse = createdResult.Value.Should().BeOfType<UserDto>().Subject;
            
            // Verify all UserDto properties
            apiResponse.Id.Should().Be(userDto.Id);
            apiResponse.FirstName.Should().Be(userDto.FirstName);
            apiResponse.LastName.Should().Be(userDto.LastName);
            apiResponse.UserName.Should().Be(userDto.UserName);
            apiResponse.Email.Should().Be(userDto.Email);
            apiResponse.Phone.Should().Be(userDto.Phone);
            apiResponse.Status.Should().Be(userDto.Status);
            apiResponse.Roles.Should().BeEquivalentTo(userDto.Roles);
        }
    }
} 