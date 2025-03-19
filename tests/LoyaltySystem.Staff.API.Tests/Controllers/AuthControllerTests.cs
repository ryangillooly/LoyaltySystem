using FluentAssertions;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Staff.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace LoyaltySystem.Staff.API.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<ILogger<AuthController>> _loggerMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _loggerMock = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(_authServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task RegisterUser_AlwaysReturnsForbiddenResponse()
        {
            // Arrange
            var request = new RegisterUserDto
            {
                FirstName = "John",
                LastName = "Doe",
                UserName = "johndoe",
                Email = "john.doe@example.com",
                Password = "P@ssw0rd!",
                ConfirmPassword = "P@ssw0rd!"
            };

            // Act
            var result = await _controller.RegisterUser(request);

            // Assert
            var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(403);
            
            // Get message property from anonymous type using reflection
            var value = statusCodeResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            messageProperty.Should().NotBeNull("because response should have 'message' property");
            
            var message = messageProperty.GetValue(value) as string;
            message.Should().Be("Registration not allowed through Staff API");
        }

        [Fact]
        public void ValidateStaffCredentials_ReturnsOkResult()
        {
            // Act
            var result = _controller.ValidateStaffCredentials();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            
            // Get message property from anonymous type using reflection
            var value = okResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            messageProperty.Should().NotBeNull("because response should have 'message' property");
            
            var message = messageProperty.GetValue(value) as string;
            message.Should().Be("Valid staff credentials");
        }
    }

    public class StaffAuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<ILogger<StaffAuthController>> _loggerMock;
        private readonly StaffAuthController _controller;

        public StaffAuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _loggerMock = new Mock<ILogger<StaffAuthController>>();
            _controller = new StaffAuthController(_authServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkWithResponse()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "john.doe@example.com",
                Password = "P@ssw0rd!"
            };

            var userDto = new UserDto
            {
                Id = "user_123",
                UserName = "johndoe",
                Email = "john.doe@example.com",
                FirstName = "John",
                LastName = "Doe",
                Roles = new List<string> { RoleType.Staff.ToString() }
            };

            var authResponse = new AuthResponseDto
            {
                Token = "valid-jwt-token",
                User = userDto
            };

            _authServiceMock.Setup(x => x.AuthenticateForAppAsync(
                    request.Email,
                    request.Password,
                    LoginIdentifierType.Email,
                    new[] { RoleType.Staff }))
                .ReturnsAsync(OperationResult<AuthResponseDto>.SuccessResult(authResponse));

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(authResponse);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "john.doe@example.com",
                Password = "WrongPassword"
            };

            _authServiceMock.Setup(x => x.AuthenticateForAppAsync(
                    request.Email,
                    request.Password,
                    LoginIdentifierType.Email,
                    new[] { RoleType.Staff }))
                .ReturnsAsync(OperationResult<AuthResponseDto>.FailureResult("Invalid username/email or password"));

            // Act
            var result = await _controller.Login(request);

            // Assert
            var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            
            // Get message property from anonymous type using reflection
            var value = unauthorizedResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            messageProperty.Should().NotBeNull("because response should have 'message' property");
            
            var message = messageProperty.GetValue(value);
            
            // Handle both string and IEnumerable<string> cases
            if (message is string messageString)
            {
                messageString.Should().Be("Invalid username/email or password");
            }
            else if (message is IEnumerable<string> messageCollection)
            {
                messageCollection.Should().Contain("Invalid username/email or password");
            }
            else
            {
                Assert.Fail("Message property is neither a string nor an IEnumerable<string>");
            }
        }

        [Fact]
        public async Task RegisterStaff_WithValidData_ReturnsCreatedWithUser()
        {
            // Arrange
            var request = new RegisterUserDto
            {
                FirstName = "John",
                LastName = "Doe",
                UserName = "johndoe",
                Email = "john.doe@example.com",
                Password = "P@ssw0rd!",
                ConfirmPassword = "P@ssw0rd!"
            };

            var userDto = new UserDto
            {
                Id = "user_123",
                UserName = "johndoe",
                Email = "john.doe@example.com",
                FirstName = "John",
                LastName = "Doe",
                Roles = new List<string> { RoleType.Staff.ToString() }
            };

            _authServiceMock.Setup(x => x.RegisterStaffAsync(request))
                .ReturnsAsync(OperationResult<UserDto>.SuccessResult(userDto));

            // Act
            var result = await _controller.RegisterStaff(request);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be("GetUserById");
            createdResult.RouteValues["id"].Should().Be(userDto.Id);
            createdResult.Value.Should().Be(userDto);
        }

        [Fact]
        public async Task RegisterStaff_WithPasswordMismatch_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterUserDto
            {
                FirstName = "John",
                LastName = "Doe",
                UserName = "johndoe",
                Email = "john.doe@example.com",
                Password = "P@ssw0rd!",
                ConfirmPassword = "DifferentPassword" // Password mismatch
            };

            // Act
            var result = await _controller.RegisterStaff(request);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            
            // Get message property from anonymous type using reflection
            var value = badRequestResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            messageProperty.Should().NotBeNull("because response should have 'message' property");
            
            var message = messageProperty.GetValue(value) as string;
            message.Should().Be("Password and confirmation password do not match");
        }

        [Fact]
        public async Task RegisterStaff_WithServiceFailure_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterUserDto
            {
                FirstName = "John",
                LastName = "Doe",
                UserName = "johndoe",
                Email = "john.doe@example.com",
                Password = "P@ssw0rd!",
                ConfirmPassword = "P@ssw0rd!"
            };

            _authServiceMock.Setup(x => x.RegisterStaffAsync(request))
                .ReturnsAsync(OperationResult<UserDto>.FailureResult("Email is already registered"));

            // Act
            var result = await _controller.RegisterStaff(request);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            
            // Get message property from anonymous type using reflection
            var value = badRequestResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            messageProperty.Should().NotBeNull("because response should have 'message' property");
            
            var message = messageProperty.GetValue(value);
            
            // Handle both string and IEnumerable<string> cases
            if (message is string messageString)
            {
                messageString.Should().Be("Email is already registered");
            }
            else if (message is IEnumerable<string> messageCollection)
            {
                messageCollection.Should().Contain("Email is already registered");
            }
            else
            {
                Assert.Fail("Message property is neither a string nor an IEnumerable<string>");
            }
        }
    }
} 