using System.Security.Cryptography;
using System.Text;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using LoyaltySystem.Domain.Interfaces;

namespace LoyaltySystem.Application.Services
{
    /// <summary>
    /// Service for authentication and user management.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;

        public AuthService(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IJwtService jwtService,
            ICustomerRepository customerRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        }

        /// <summary>
        /// Authenticates a user using either username or email, and returns a JWT token.
        /// </summary>
        public async Task<OperationResult<AuthResponseDto>> AuthenticateAsync(string identifier, string password, LoginIdentifierType identifierType)
        {
            User? user;
            
            switch (identifierType)
            {
                case LoginIdentifierType.Email:
                    user = await _userRepository.GetByEmailAsync(identifier);
                    break;
                    
                case LoginIdentifierType.Username:
                    user = await _userRepository.GetByUsernameAsync(identifier);
                    break;
                    
                default:
                    return OperationResult<AuthResponseDto>.FailureResult("Invalid identifier type");
            }
            
            if (user is null)
                return OperationResult<AuthResponseDto>.FailureResult("Invalid username/email or password");
                
            if (user.Status != UserStatus.Active)
                return OperationResult<AuthResponseDto>.FailureResult("User account is not active");
                
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return OperationResult<AuthResponseDto>.FailureResult("Invalid username/email or password");
            
            user.RecordLogin();
            await _userRepository.UpdateAsync(user);
            
            var token = GenerateJwtToken(user);
            
            return OperationResult<AuthResponseDto>.SuccessResult(new AuthResponseDto
            {
                Token = token,
                User = MapUserToDto(user)
            });
        }
        
        /// <summary>
        /// Authenticates a user for a specific application, checking they have the required roles.
        /// </summary>
        public async Task<OperationResult<AuthResponseDto>> AuthenticateForAppAsync(
            string identifier, 
            string password, 
            LoginIdentifierType identifierType,
            IEnumerable<RoleType> allowedRoles)
        {
            // First authenticate the user
            var authResult = await AuthenticateAsync(identifier, password, identifierType);
            if (!authResult.Success)
                return authResult;
            
            // Get detailed user info
            var user = await _userRepository.GetByIdAsync(authResult.Data.User.Id);
            if (user is null)
                return OperationResult<AuthResponseDto>.FailureResult("User not found");
                
            // Check if user has any of the allowed roles
            var userRoles = user.Roles.Select(r => r.Role).ToList();
            if (!userRoles.Any(role => allowedRoles.Contains(role)))
            {
                return OperationResult<AuthResponseDto>.FailureResult(
                    "You don't have permission to access this application");
            }
            
            // User is authorized for this application
            return authResult;
        }
        
        /// <summary>
        /// Private helper method to create a user with a specific role
        /// </summary>
        private async Task<OperationResult<User>> CreateUserAsync(RegisterUserDto registerDto, RoleType role)
        {
            try
            {
                // Validate username and email
                if (string.IsNullOrEmpty(registerDto.UserName))
                    return OperationResult<User>.FailureResult("Username is required");
                    
                if (string.IsNullOrEmpty(registerDto.Email))
                    return OperationResult<User>.FailureResult("Email is required");
                    
                var existingUsername = await _userRepository.GetByUsernameAsync(registerDto.UserName);
                if (existingUsername is { })
                    return OperationResult<User>.FailureResult("Username already exists");
                
                var existingEmail = await _userRepository.GetByEmailAsync(registerDto.Email);
                if (existingEmail is { })
                    return OperationResult<User>.FailureResult("Email already exists");
                
                // Create password hash
                CreatePasswordHash(registerDto.Password, out string passwordHash, out string passwordSalt);
                
                // Create user with appropriate role
                var user = new User(
                    registerDto.FirstName, 
                    registerDto.LastName, 
                    registerDto.UserName, 
                    registerDto.Email, 
                    passwordHash, 
                    passwordSalt);

                user.CustomerId = null;
                user.AddRole(role);
                
                // Save the user
                await _userRepository.AddAsync(user);
                
                return OperationResult<User>.SuccessResult(user);
            }
            catch (Exception ex)
            {
                return OperationResult<User>.FailureResult(ex.Message);
            }
        }

        /// <summary>
        /// Registers a customer user
        /// </summary>
        public async Task<OperationResult<UserDto>> RegisterCustomerAsync(RegisterUserDto registerDto)
        {
            // Create the user with Customer role
            var userResult = await CreateUserAsync(registerDto, RoleType.Customer);
            if (!userResult.Success)
                return OperationResult<UserDto>.FailureResult(userResult.Errors);
                
            var user = userResult.Data;
            
            try
            {
                // Create the customer record and link it
                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var customer = new Customer(
                        null,
                        registerDto.FirstName,
                        registerDto.LastName,
                        registerDto.UserName,
                        registerDto.Email,
                        registerDto.Phone,
                        null,
                        false
                    );
                        
                    // Save the customer and link to user
                    customer = await _customerRepository.AddAsync(customer, _unitOfWork.CurrentTransaction);
                    user.LinkToCustomer(customer.Id.ToString());
                        
                    // Update the user with the customer link
                    await _userRepository.UpdateAsync(user);
                });
                    
                return OperationResult<UserDto>.SuccessResult(MapUserToDto(user));
            }
            catch (Exception ex)
            {
                return OperationResult<UserDto>.FailureResult(ex.Message);
            }
        }
        
        /// <summary>
        /// Registers a staff user
        /// </summary>
        public async Task<OperationResult<UserDto>> RegisterStaffAsync(RegisterUserDto registerDto)
        {
            // Create the user with Staff role
            var userResult = await CreateUserAsync(registerDto, RoleType.Staff);
            if (!userResult.Success)
                return OperationResult<UserDto>.FailureResult(userResult.Errors);
            
            return OperationResult<UserDto>.SuccessResult(MapUserToDto(userResult.Data));
        }
        
        /// <summary>
        /// Registers an admin user
        /// </summary>
        public async Task<OperationResult<UserDto>> RegisterAdminAsync(RegisterUserDto registerDto)
        {
            // Create the user with Admin role
            var userResult = await CreateUserAsync(registerDto, RoleType.Admin);
            if (!userResult.Success)
                return OperationResult<UserDto>.FailureResult(userResult.Errors);
            
            return OperationResult<UserDto>.SuccessResult(MapUserToDto(userResult.Data));
        }
        
        /// <summary>
        /// Adds customer role to an existing user
        /// </summary>
        public async Task<OperationResult<UserDto>> AddCustomerRoleToUserAsync(string userId)
        {
            // Get existing user
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
                return OperationResult<UserDto>.FailureResult("User not found");
            
            try
            {
                // Check if user already has customer role
                if (!user.HasRole(RoleType.Customer))
                {
                    // Add the Customer role
                    user.AddRole(RoleType.Customer);
                    await _userRepository.AddRoleAsync(userId, RoleType.Customer);
                }
                
                // Create customer entity if needed
                if (user.CustomerId is null)
                {
                    await _unitOfWork.ExecuteInTransactionAsync(async () =>
                    {
                        var customer = new Customer(
                            null,
                            user.FirstName,
                            user.LastName,
                            user.UserName,
                            user.Email,
                            null, // Phone may not be available
                            null, // Address may not be available
                            false // Default marketing consent
                        );
                        
                        // Save the customer and link to user
                        customer = await _customerRepository.AddAsync(customer, _unitOfWork.CurrentTransaction);
                        user.LinkToCustomer(customer.Id.ToString());
                        
                        // Update the user with the customer link
                        await _userRepository.UpdateAsync(user);
                    });
                }
                
                return OperationResult<UserDto>.SuccessResult(MapUserToDto(user));
            }
            catch (Exception ex)
            {
                return OperationResult<UserDto>.FailureResult(ex.Message);
            }
        }

        /// <summary>
        /// Updates a user's profile.
        /// </summary>
        public async Task<OperationResult<UserDto>> UpdateProfileAsync(string userId, UpdateProfileDto updateDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user is null)
                return OperationResult<UserDto>.FailureResult("User not found");
            
            // Check if username is changing and already exists
            if (!string.IsNullOrEmpty(updateDto.UserName) && updateDto.UserName != user.UserName)
            {
                var existingUsername = await _userRepository.GetByUsernameAsync(updateDto.UserName);
                if (existingUsername is { } && existingUsername.Id.ToString() != userId)
                    return OperationResult<UserDto>.FailureResult("Username already exists");
                
                user.UpdateUserName(updateDto.UserName);
            }
                
            // Check if email is changing and already exists
            if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != user.Email)
            {
                var existingEmail = await _userRepository.GetByEmailAsync(updateDto.Email);
                if (existingEmail is { } && existingEmail.Id.ToString() != userId)
                    return OperationResult<UserDto>.FailureResult("Email already exists");
                    
                user.UpdateEmail(updateDto.Email);
            }
            
            // Update password if provided
            if (!string.IsNullOrEmpty(updateDto.NewPassword))
            {
                if (!VerifyPasswordHash(updateDto.CurrentPassword, user.PasswordHash, user.PasswordSalt))
                    return OperationResult<UserDto>.FailureResult("Current password is incorrect");
                    
                CreatePasswordHash(updateDto.NewPassword, out string passwordHash, out string passwordSalt);
                user.UpdatePassword(passwordHash, passwordSalt);
            }
            
            await _userRepository.UpdateAsync(user);
            
            return OperationResult<UserDto>.SuccessResult(MapUserToDto(user));
        }

        /// <summary>
        /// Gets a user by ID.
        /// </summary>
        public async Task<OperationResult<UserDto>> GetUserByIdAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user is null)
                return OperationResult<UserDto>.FailureResult("User not found");
                
            return OperationResult<UserDto>.SuccessResult(MapUserToDto(user));
        }

        /// <summary>
        /// Gets a user by customer ID.
        /// </summary>
        public async Task<OperationResult<UserDto>> GetUserByCustomerIdAsync(string customerId)
        {
            var user = await _userRepository.GetByCustomerIdAsync(customerId);
            
            if (user is null)
                return OperationResult<UserDto>.FailureResult("User not found");
                
            return OperationResult<UserDto>.SuccessResult(MapUserToDto(user));
        }

        /// <summary>
        /// Adds a role to a user.
        /// </summary>
        public async Task<OperationResult<UserDto>> AddRoleAsync(string userId, RoleType role)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user is null)
                return OperationResult<UserDto>.FailureResult("User not found");
                
            user.AddRole(role);
            await _userRepository.AddRoleAsync(userId, role);
            
            return OperationResult<UserDto>.SuccessResult(MapUserToDto(user));
        }

        /// <summary>
        /// Removes a role from a user.
        /// </summary>
        public async Task<OperationResult<UserDto>> RemoveRoleAsync(string userId, RoleType role)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user is null)
                return OperationResult<UserDto>.FailureResult("User not found");
                
            user.RemoveRole(role);
            await _userRepository.RemoveRoleAsync(userId, role);
            
            return OperationResult<UserDto>.SuccessResult(MapUserToDto(user));
        }

        /// <summary>
        /// Links a user to a customer.
        /// </summary>
        public async Task<OperationResult<UserDto>> LinkCustomerAsync(string userId, string customerId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user is null)
                return OperationResult<UserDto>.FailureResult("User not found");
                
            // Check if customer already linked to a user
            var existingUser = await _userRepository.GetByCustomerIdAsync(customerId);
            if (existingUser is { } && existingUser.Id.ToString() != userId)
                return OperationResult<UserDto>.FailureResult("Customer already linked to another user");
                
            user.LinkToCustomer(customerId);
            await _userRepository.UpdateAsync(user);
            
            return OperationResult<UserDto>.SuccessResult(MapUserToDto(user));
        }

        #region Helper Methods

        private string GenerateJwtToken(User user)
        {
            // Create additional claims if needed
            var additionalClaims = new Dictionary<string, string>();
            
            // Add customer ID claim if it exists
            if (user.CustomerId is { })
            {
                additionalClaims.Add("CustomerId", user.CustomerId.ToString());
                additionalClaims.Add("UserId", user.Id.ToString());
            }
            
            // Get the user's primary role (or default if none)
            var roles = user.Roles.Any() 
                ? user.Roles.Select(r => r.Role.ToString()).ToList() 
                : new List<string>{ "User" };
                
            // Use the JwtService to generate the token
            return _jwtService.GenerateToken(
                user.Id.ToString(),
                user.FirstName,
                user.LastName,
                user.Email,
                roles,
                additionalClaims
            );
        }

        private static bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
        {
            byte[] saltBytes = Convert.FromBase64String(storedSalt);
            using var hmac = new HMACSHA512(saltBytes);
            
            var computedHash = Convert.ToBase64String(
                hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        
            return computedHash == storedHash;
        }
        private static void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using var hmac = new HMACSHA512();
            
            passwordSalt = Convert.ToBase64String(hmac.Key);
            passwordHash = Convert.ToBase64String(
                hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        private static UserDto MapUserToDto(User user) =>
            new ()
            {
                Id = user.Id.ToString(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                Status = user.Status.ToString(),
                CustomerId = user.CustomerId?.ToString() ?? string.Empty,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = user.Roles.Select(r => r.Role.ToString()).ToList()
            };

        #endregion
    }
} 