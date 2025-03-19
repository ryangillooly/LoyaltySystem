using FluentAssertions;
using LoyaltySystem.Customer.API.Controllers;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Shared.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace LoyaltySystem.Customer.API.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<ICustomerService> _customerServiceMock;
        private readonly Mock<ILogger<AuthController>> _loggerMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _customerServiceMock = new Mock<ICustomerService>();
            _loggerMock = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(_authServiceMock.Object, _customerServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var request = new RegisterUserDto
            {
                FirstName = "John",
                LastName = "Doe",
                UserName = "johndoe",
                Email = "john.doe@example.com",
                Password = "P@ssw0rd!",
                ConfirmPassword = "P@ssw0rd!",
                Phone = "1234567890"
            };

            var userDto = new UserDto
            {
                Id = new UserId().ToString(),
                FirstName = "John",
                LastName = "Doe",
                UserName = "johndoe",
                Email = "john.doe@example.com",
                Phone = "1234567890",
                Status = UserStatus.Active.ToString(),
                Roles = new List<string> { RoleType.Customer.ToString() }
            };
            
            var serviceResult = OperationResult<UserDto>.SuccessResult(userDto);

            _authServiceMock.Setup(x => 
                x.RegisterCustomerAsync(It.IsAny<RegisterUserDto>()))
                    .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.RegisterUser(request);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.StatusCode.Should().Be(201);
            
            var returnedDto = createdResult.Value.Should().BeAssignableTo<UserDto>().Subject;
            returnedDto.Id.Should().Be(userDto.Id);
            returnedDto.FirstName.Should().Be(userDto.FirstName);
            returnedDto.LastName.Should().Be(userDto.LastName);
            returnedDto.UserName.Should().Be(userDto.UserName);
            returnedDto.Email.Should().Be(userDto.Email);
            returnedDto.Phone.Should().Be(userDto.Phone);
            returnedDto.Status.Should().Be(userDto.Status);
            
            // Verify roles
            returnedDto.Roles.Should().BeEquivalentTo(userDto.Roles);
            
            // Additional assertions for other properties if needed
            // returnedDto.CustomerId.Should().Be(userDto.CustomerId);
            // returnedDto.CreatedAt.Should().Be(userDto.CreatedAt);
        }

        [Fact]
        public async Task Register_WithPasswordMismatch_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterUserDto
            {
                FirstName = "John",
                LastName = "Doe",
                UserName = "johndoe",
                Email = "john.doe@example.com",
                Password = "P@ssw0rd!",
                ConfirmPassword = "DifferentPassword", // Password mismatch
                Phone = "1234567890"
            };

            // Act
            var result = await _controller.RegisterUser(request);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            
            // Check that the response value is not null and contains an error message about passwords
            badRequestResult.Value.Should().NotBeNull();
            badRequestResult.Value.ToString().Should().Contain("Password");
        }

        [Fact]
        public async Task Register_WithServiceFailure_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterUserDto
            {
                FirstName = "John",
                LastName = "Doe",
                UserName = "johndoe",
                Email = "john.doe@example.com",
                Password = "P@ssw0rd!",
                ConfirmPassword = "P@ssw0rd!",
                Phone = "1234567890"
            };

            var errorMessage = "Email is already registered";
            var serviceResult = OperationResult<UserDto>.FailureResult(errorMessage);

            _authServiceMock.Setup(x => x.RegisterCustomerAsync(It.IsAny<RegisterUserDto>()))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.RegisterUser(request);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            
            badRequestResult.Value.Should().NotBeNull();
            
            var messageProperty = badRequestResult.Value.GetType().GetProperty("message");
            messageProperty.Should().NotBeNull("Response should have a 'message' property");
            
            var messageValue = messageProperty?.GetValue(badRequestResult.Value);
            messageValue.Should().NotBeNull("Message property should not be null");
            
            messageValue.Should().BeAssignableTo<IEnumerable<string>>("Message should be a collection of strings");
            
            var messageStrings = (IEnumerable<string>)messageValue;
            messageStrings.Should().Contain(errorMessage, $"Message collection should contain '{errorMessage}'");
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "john.doe@example.com",
                UserName = "johndoe",
                Password = "P@ssw0rd!"
            };

            var userDto = new UserDto
            {
                Id = new UserId().ToString(),
                UserName = "johndoe",
                Email = "john.doe@example.com",
                Roles = new List<string> { RoleType.Customer.ToString() }
            };

            var authResponseDto = new AuthResponseDto
            {
                Token = "valid-jwt-token",
                User = userDto
            };

            var serviceResult = OperationResult<AuthResponseDto>.SuccessResult(authResponseDto);

            _authServiceMock.Setup(x => x.AuthenticateAsync(
                    request.Email, 
                    request.Password, 
                    LoginIdentifierType.Email))
                .ReturnsAsync(serviceResult);

            // Also mock the GetUserByIdAsync method that's called by HasRoles
            _authServiceMock.Setup(x => x.GetUserByIdAsync(userDto.Id))
                .ReturnsAsync(OperationResult<UserDto>.SuccessResult(userDto));

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<AuthResponseDto>().Subject;
            response.Token.Should().Be(authResponseDto.Token);
            response.User.Id.Should().Be(userDto.Id);
            response.User.UserName.Should().Be(userDto.UserName);
            response.User.Email.Should().Be(userDto.Email);
            response.User.Roles.Should().BeEquivalentTo(userDto.Roles);
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

            var serviceResult = OperationResult<AuthResponseDto>.FailureResult("Invalid username/email or password");

            _authServiceMock.Setup(x => x.AuthenticateAsync(
                    request.Email,
                    request.Password,
                    LoginIdentifierType.Email))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            var response = unauthorizedResult.Value.Should().BeAssignableTo<object>().Subject;
            response.Should().NotBeNull();
        }

        [Fact]
        public async Task Login_WithDeactivatedAccount_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "john.doe@example.com",
                Password = "P@ssw0rd!"
            };

            var serviceResult = OperationResult<AuthResponseDto>.FailureResult("User account is not active");

            _authServiceMock.Setup(x => x.AuthenticateAsync(
                    request.Email,
                    request.Password,
                    LoginIdentifierType.Email))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            var response = unauthorizedResult.Value.Should().BeAssignableTo<object>().Subject;
            response.Should().NotBeNull();
        }

        [Fact]
        public async Task Login_WithWrongRole_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "john.doe@example.com",
                Password = "P@ssw0rd!"
            };

            var serviceResult = OperationResult<AuthResponseDto>.FailureResult("You don't have permission to access this application");

            _authServiceMock.Setup(x => x.AuthenticateAsync(
                    request.Email,
                    request.Password,
                    LoginIdentifierType.Email))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            var response = unauthorizedResult.Value.Should().BeAssignableTo<object>().Subject;
            response.Should().NotBeNull();
        }
    }
} 