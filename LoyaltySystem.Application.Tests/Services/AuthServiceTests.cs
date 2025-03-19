using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using FluentAssertions;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Application.Services;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Interfaces;
using LoyaltySystem.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Language.Flow;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace LoyaltySystem.Application.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _jwtServiceMock = new Mock<IJwtService>();
            
            _authService = new AuthService(
                _userRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _jwtServiceMock.Object,
                _customerRepositoryMock.Object);
        }

        [Fact]
        public async Task RegisterCustomerAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                FirstName = "John",
                LastName = "Doe",
                UserName = "johndoe",
                Email = "john@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(registerDto.Email))
                .ReturnsAsync((User)null);

            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync(registerDto.UserName))
                .ReturnsAsync((User)null);

            _userRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<DbTransaction>()))
                .Returns(Task.CompletedTask);

            var mockTransaction = new Mock<DbTransaction>();
            _unitOfWorkMock
                .Setup(x => x.BeginTransactionAsync())
                .ReturnsAsync(mockTransaction.Object);

            _unitOfWorkMock
                .Setup(x => x.CommitAsync(It.IsAny<DbTransaction>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.RegisterCustomerAsync(registerDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.FirstName.Should().Be(registerDto.FirstName);
            result.Data.LastName.Should().Be(registerDto.LastName);
            result.Data.Email.Should().Be(registerDto.Email);
        }

        [Fact]
        public async Task RegisterCustomerAsync_WithExistingEmail_ReturnsFailure()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                FirstName = "John",
                LastName = "Doe",
                UserName = "johndoe",
                Email = "john@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var existingUser = new User(
                "John",
                "Doe",
                "existinguser",
                "john@example.com",
                "passwordHash",
                "passwordSalt");

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(registerDto.Email))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _authService.RegisterCustomerAsync(registerDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Contains("Email"));
        }

        [Fact]
        public async Task AuthenticateAsync_WithValidCredentials_ReturnsSuccessAndToken()
        {
            // Arrange
            var email = "test@example.com";
            var password = "Password123!";

            // Create a password hash using HMACSHA512 instead of BCrypt
            string passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            var user = new User(
                "Test",
                "User",
                "testuser",
                email,
                passwordHash,
                passwordSalt);

            user.AddRole(RoleType.Customer);

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);

            _jwtServiceMock
                .Setup(x => x.GenerateToken(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<Dictionary<string, string>>()))
                .Returns("generatedToken");

            // Act
            var result = await _authService.AuthenticateAsync(email, password, LoginIdentifierType.Email);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Token.Should().Be("generatedToken");
            result.Data.User.Should().NotBeNull();
            result.Data.User.Email.Should().Be(email);
        }

        [Fact]
        public async Task AuthenticateAsync_WithInvalidCredentials_ReturnsFailure()
        {
            // Arrange
            var email = "test@example.com";
            var password = "WrongPassword";

            // Create a password hash using HMACSHA512 instead of BCrypt
            string passwordHash, passwordSalt;
            CreatePasswordHash("Password123!", out passwordHash, out passwordSalt);

            var user = new User(
                "Test",
                "User",
                "testuser",
                email,
                passwordHash,
                passwordSalt);

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.AuthenticateAsync(email, password, LoginIdentifierType.Email);

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Contains("Invalid"));
        }

        [Fact]
        public async Task AuthenticateAsync_WithNonExistentUser_ReturnsFailure()
        {
            // Arrange
            var email = "nonexistent@example.com";
            var password = "Password123!";

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync((User)null);

            // Act
            var result = await _authService.AuthenticateAsync(email, password, LoginIdentifierType.Email);

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Contains("Invalid username/email or password"));
        }

        // Helper method to create password hash for testing
        private static void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using var hmac = new HMACSHA512();
            
            passwordSalt = Convert.ToBase64String(hmac.Key);
            passwordHash = Convert.ToBase64String(
                hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        // Helper method to verify password hash for testing
        private static bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
        {
            byte[] saltBytes = Convert.FromBase64String(storedSalt);
            using var hmac = new HMACSHA512(saltBytes);
            
            var computedHash = Convert.ToBase64String(
                hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        
            return computedHash == storedHash;
        }
    }
} 