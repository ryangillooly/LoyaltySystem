using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LoyaltySystem.Admin.API.Controllers;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Application.Services;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Interfaces;
using LoyaltySystem.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace LoyaltySystem.Admin.API.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly AuthService _authService;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly AuthController _controller;
        private readonly ITestOutputHelper _output;

        public AuthControllerTests(ITestOutputHelper output)
        {
            _output = output;
            
            // Create mocks for all the dependencies of AuthService
            _mockUserRepository = new Mock<IUserRepository>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockJwtService = new Mock<IJwtService>();
            
            // Create a real instance of AuthService with mocked dependencies
            _authService = new AuthService(
                _mockUserRepository.Object,
                _mockConfiguration.Object,
                _mockUnitOfWork.Object,
                _mockJwtService.Object
            );
            
            _mockLogger = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(_authService, _mockLogger.Object);
        }

        #region Login Tests

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var password = "password123";
            var loginDto = new LoginRequestDto
            {
                Username = "admin",
                Password = password
            };

            // Create a valid mock user with properly hashed password that will verify correctly
            CreatePasswordHash(password, out string passwordHash, out string passwordSalt);
            var user = new User(
                loginDto.Username,
                "admin@example.com",
                passwordHash,
                passwordSalt);
                
            user.RecordLogin(); // Make sure the user is active
            
            // Setup dependencies to simulate successful authentication:
            
            // 1. Repository returns our active user
            _mockUserRepository
                .Setup<Task<User>>(x => x.GetByUsernameAsync(loginDto.Username))
                .ReturnsAsync(user);
                                
            // 2. Setup JWT token generation
            _mockJwtService
                .Setup(x => x.GenerateToken(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<IDictionary<string, string>>()))
                .Returns("jwt-token");

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
            
            // Check that the response has the expected structure
            if (okResult.Value is AuthResponseDto returnedResponse)
            {
                returnedResponse.Token.Should().Be("jwt-token");
                returnedResponse.User.Should().NotBeNull();
                returnedResponse.User.Username.Should().Be(loginDto.Username);
            }
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                Username = "admin",
                Password = "wrongpassword"
            };

            // Setup repository to simulate nonexistent user
            _mockUserRepository
                .Setup<Task<User>>(x => x.GetByUsernameAsync(loginDto.Username))
                .ReturnsAsync((User)null);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorizedResult.Value.Should().NotBeNull();
            
            // Verify the error message using serialization
            var json = JsonSerializer.Serialize(unauthorizedResult.Value);
            json.Should().Contain("Invalid username or password");
        }

        #endregion

        #region Register Tests

        [Fact]
        public async Task Register_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Username = "newadmin",
                Email = "newadmin@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            // Setup repository for successful registration
            _mockUserRepository
                .Setup<Task<User>>(x => x.GetByUsernameAsync(registerDto.Username))
                .ReturnsAsync((User)null);

            _mockUserRepository
                .Setup<Task<User>>(x => x.GetByEmailAsync(registerDto.Email))
                .ReturnsAsync((User)null);

            _mockUserRepository
                .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<System.Data.IDbTransaction>()))
                .Returns(Task.CompletedTask);

            // Setup UnitOfWork
            _mockUnitOfWork
                .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns(Task.CompletedTask);
                
            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var createdAtActionResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be(nameof(AuthController.GetUserById));
            createdAtActionResult.Value.Should().NotBeNull();
            
            if (createdAtActionResult.Value is UserDto userDto)
            {
                userDto.Username.Should().Be(registerDto.Username);
                userDto.Email.Should().Be(registerDto.Email);
                userDto.Id.Should().StartWith("usr_");
            }
        }

        [Fact]
        public async Task Register_WithPasswordMismatch_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Username = "newadmin",
                Email = "newadmin@example.com",
                Password = "password123",
                ConfirmPassword = "password456" // Mismatch
            };

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().NotBeNull();
            
            // Verify the error message
            var json = JsonSerializer.Serialize(badRequestResult.Value);
            json.Should().Contain("Password and confirmation password do not match");
        }

        [Fact]
        public async Task Register_WithExistingUsername_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Username = "existingadmin",
                Email = "newadmin@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            // Setup UserRepository to return an existing user with the same username
            var existingUser = CreateMockUser("existingadmin", "existing@example.com");
            
            _mockUserRepository
                .Setup<Task<User>>(x => x.GetByUsernameAsync(registerDto.Username))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            badRequestResult.Value.Should().NotBeNull();

            // Output the actual value for debugging
            _output.WriteLine($"Bad request value: {badRequestResult.Value}");
            
            // Verify the error message
            var json = JsonSerializer.Serialize(badRequestResult.Value);
            json.Should().Contain("Username already exists");
        }

        #endregion

        #region Profile Tests

        [Fact]
        public async Task GetProfile_WithValidUserId_ReturnsOkResult()
        {
            // Arrange - Setup a valid claim with a properly formatted UserId
            var validUserId = GenerateValidUserIdString();
            SetupClaimsPrincipal(validUserId);

            // Create mock user to be returned by repository
            var user = CreateMockUser("admin", "admin@example.com");
            
            // Setup UserRepository to return our user for any string input
            _mockUserRepository
                .Setup<Task<User>>(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetProfile();

            // Assert the result behavior
            result.Should().BeOfType<OkObjectResult>();
            if (result is OkObjectResult okResult)
            {
                okResult.Value.Should().BeOfType<UserDto>();
                var returnedDto = okResult.Value as UserDto;
                returnedDto.Username.Should().Be(user.Username);
            }
        }

        [Fact]
        public async Task GetProfile_WithInvalidUserId_ReturnsUnauthorized()
        {
            // Arrange - Setup an invalid claim format that would cause parsing to fail
            SetupClaimsPrincipal("invalid-id"); // Not in the expected format
            
            // Act
            var result = await _controller.GetProfile();

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = (UnauthorizedObjectResult)result;
            unauthorizedResult.Value.Should().NotBeNull();
            
            // Verify the error message
            var json = JsonSerializer.Serialize(unauthorizedResult.Value);
            json.Should().Contain("Invalid user identification");
        }

        #endregion

        #region Admin Specific Tests

        [Fact]
        public async Task AddRole_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var validUserId = GenerateValidUserIdString();
            var roleDto = new UserRoleDto
            {
                UserId = validUserId,
                Role = RoleType.Admin.ToString()
            };

            // Setup controller context
            SetupControllerContext();

            // Create a mock user to be returned by the repository
            var user = CreateMockUser("user", "user@example.com");
            
            // Setup the repository
            _mockUserRepository
                .Setup<Task<User>>(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
                
            _mockUserRepository
                .Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddRole(roleDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Replicates the password hashing logic from AuthService
        /// </summary>
        private void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using var hmac = new HMACSHA512();
            
            passwordSalt = Convert.ToBase64String(hmac.Key);
            passwordHash = Convert.ToBase64String(
                hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        /// <summary>
        /// Creates a mock User object with the given username and email
        /// </summary>
        private User CreateMockUser(string username, string email)
        {
            // Create a valid Base64 string for password hash and salt
            // This is important because the VerifyPasswordHash method expects valid Base64
            // but we're not trying to match a specific password with these
            string validPasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("hashedpassword"));
            string validSalt = Convert.ToBase64String(Encoding.UTF8.GetBytes("salt"));
            
            // Create a proper User object with valid Base64 strings
            var user = new User(
                username,
                email,
                validPasswordHash,
                validSalt);
                
            return user;
        }

        /// <summary>
        /// Generates a valid UserId string in the correct format (usr_xxxx...)
        /// </summary>
        private string GenerateValidUserIdString()
        {
            // Create a proper UserId in the format expected by the system
            // Format is usr_ followed by Base64 encoded GUID
            var guid = Guid.NewGuid();
            string encoded = Convert.ToBase64String(guid.ToByteArray())
                .Replace("/", "_")
                .Replace("+", "-")
                .Replace("=", "");
            return $"usr_{encoded}";
        }

        /// <summary>
        /// Sets up controller context with claims principal
        /// </summary>
        private void SetupClaimsPrincipal(string userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
        }

        /// <summary>
        /// Sets up a basic controller context
        /// </summary>
        private void SetupControllerContext()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }
        
        #endregion
    }
}
