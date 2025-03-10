using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LoyaltySystem.Application.Services
{
    /// <summary>
    /// Service for authentication and user management.
    /// </summary>
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(
            IUserRepository userRepository,
            IConfiguration configuration,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        public async Task<Result<AuthResponseDto>> AuthenticateAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            
            if (user == null)
                return Result<AuthResponseDto>.Failure("Invalid username or password");
                
            if (user.Status != UserStatus.Active)
                return Result<AuthResponseDto>.Failure("User account is not active");
                
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return Result<AuthResponseDto>.Failure("Invalid username or password");
                
            // Record login
            user.RecordLogin();
            await _userRepository.UpdateAsync(user);
            
            // Generate token
            var token = GenerateJwtToken(user);
            
            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                Token = token,
                User = MapToDto(user)
            });
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        public async Task<Result<UserDto>> RegisterAsync(RegisterUserDto registerDto)
        {
            // Check if username already exists
            var existingUsername = await _userRepository.GetByUsernameAsync(registerDto.Username);
            if (existingUsername != null)
                return Result<UserDto>.Failure("Username already exists");
                
            // Check if email already exists
            var existingEmail = await _userRepository.GetByEmailAsync(registerDto.Email);
            if (existingEmail != null)
                return Result<UserDto>.Failure("Email already exists");
                
            // Create password hash
            CreatePasswordHash(registerDto.Password, out string passwordHash, out string passwordSalt);
            
            // Create user entity
            var user = new User(
                registerDto.Username,
                registerDto.Email,
                passwordHash,
                passwordSalt);
                
            // Add default customer role
            user.AddRole(RoleType.Customer);
            
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _userRepository.AddAsync(user);
            });
            
            return Result<UserDto>.Success(MapToDto(user));
        }

        /// <summary>
        /// Updates a user's profile.
        /// </summary>
        public async Task<Result<UserDto>> UpdateProfileAsync(UserId userId, UpdateProfileDto updateDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
                return Result<UserDto>.Failure("User not found");
                
            // Check if email is changing and already exists
            if (updateDto.Email != user.Email)
            {
                var existingEmail = await _userRepository.GetByEmailAsync(updateDto.Email);
                if (existingEmail != null)
                    return Result<UserDto>.Failure("Email already exists");
                    
                user.UpdateEmail(updateDto.Email);
            }
            
            // Update password if provided
            if (!string.IsNullOrEmpty(updateDto.NewPassword))
            {
                if (!VerifyPasswordHash(updateDto.CurrentPassword, user.PasswordHash, user.PasswordSalt))
                    return Result<UserDto>.Failure("Current password is incorrect");
                    
                CreatePasswordHash(updateDto.NewPassword, out string passwordHash, out string passwordSalt);
                user.UpdatePassword(passwordHash, passwordSalt);
            }
            
            await _userRepository.UpdateAsync(user);
            
            return Result<UserDto>.Success(MapToDto(user));
        }

        /// <summary>
        /// Gets a user by ID.
        /// </summary>
        public async Task<Result<UserDto>> GetUserByIdAsync(UserId userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
                return Result<UserDto>.Failure("User not found");
                
            return Result<UserDto>.Success(MapToDto(user));
        }

        /// <summary>
        /// Gets a user by customer ID.
        /// </summary>
        public async Task<Result<UserDto>> GetUserByCustomerIdAsync(CustomerId customerId)
        {
            var user = await _userRepository.GetByCustomerIdAsync(customerId);
            
            if (user == null)
                return Result<UserDto>.Failure("User not found");
                
            return Result<UserDto>.Success(MapToDto(user));
        }

        /// <summary>
        /// Adds a role to a user.
        /// </summary>
        public async Task<Result<UserDto>> AddRoleAsync(UserId userId, RoleType role)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
                return Result<UserDto>.Failure("User not found");
                
            user.AddRole(role);
            await _userRepository.AddRoleAsync(userId, role);
            
            return Result<UserDto>.Success(MapToDto(user));
        }

        /// <summary>
        /// Removes a role from a user.
        /// </summary>
        public async Task<Result<UserDto>> RemoveRoleAsync(UserId userId, RoleType role)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
                return Result<UserDto>.Failure("User not found");
                
            user.RemoveRole(role);
            await _userRepository.RemoveRoleAsync(userId, role);
            
            return Result<UserDto>.Success(MapToDto(user));
        }

        /// <summary>
        /// Links a user to a customer.
        /// </summary>
        public async Task<Result<UserDto>> LinkCustomerAsync(UserId userId, CustomerId customerId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
                return Result<UserDto>.Failure("User not found");
                
            // Check if customer already linked to a user
            var existingUser = await _userRepository.GetByCustomerIdAsync(customerId);
            if (existingUser != null && existingUser.Id != userId)
                return Result<UserDto>.Failure("Customer already linked to another user");
                
            user.LinkToCustomer(customerId);
            await _userRepository.UpdateAsync(user);
            
            return Result<UserDto>.Success(MapToDto(user));
        }

        #region Helper Methods

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };
            
            // Add roles as claims
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Role.ToString()));
            }
            
            // Add customer ID claim if it exists
            if (user.CustomerId != null)
            {
                claims.Add(new Claim("CustomerId", user.CustomerId.ToString()));
            }
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = Convert.ToBase64String(hmac.Key);
                passwordHash = Convert.ToBase64String(
                    hmac.ComputeHash(Encoding.UTF8.GetBytes(password))
                );
            }
        }

        private static bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
        {
            var saltBytes = Convert.FromBase64String(storedSalt);
            
            using (var hmac = new HMACSHA512(saltBytes))
            {
                var computedHash = Convert.ToBase64String(
                    hmac.ComputeHash(Encoding.UTF8.GetBytes(password))
                );
                
                return computedHash == storedHash;
            }
        }

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CustomerId = user.CustomerId,
                Status = user.Status.ToString(),
                Roles = user.Roles.Select(r => r.Role.ToString()).ToList(),
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }

        #endregion
    }
} 