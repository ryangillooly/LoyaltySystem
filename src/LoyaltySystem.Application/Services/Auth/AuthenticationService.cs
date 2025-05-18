using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Application.Interfaces.Auth;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Interfaces;
using LoyaltySystem.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace LoyaltySystem.Application.Services.Auth;

public class AuthenticationService : IAuthenticationService 
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationService"/> class with required dependencies.
    /// </summary>
    /// <param name="userRepository">Repository for accessing user data.</param>
    /// <param name="jwtService">Service for generating JWT tokens.</param>
    /// <param name="passwordHasher">Hasher for verifying user passwords.</param>
    /// <param name="logger">Logger for authentication events.</param>
    /// <exception cref="ArgumentNullException">Thrown if any dependency is null.</exception>
    public AuthenticationService
    (
        IUserRepository userRepository,
        IJwtService jwtService,
        IPasswordHasher<User> passwordHasher,
        ILogger logger
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }
    
    /// <summary>
    /// Authenticates a user by verifying credentials and returns a JWT token upon successful authentication.
    /// </summary>
    /// <param name="dto">The login request containing user identifier and password.</param>
    /// <returns>An operation result containing the login response with access token if authentication succeeds; otherwise, a failure result with an error message.</returns>
    public async Task<OperationResult<LoginResponseDto>> AuthenticateAsync(LoginRequestDto dto)
    {
        User? user = await GetUserByEmailOrUsernameAsync(dto);
            
        if (user is null)
            return OperationResult<LoginResponseDto>.FailureResult("Invalid username/email or password");
                
        if (user.Status != UserStatus.Active)
            return OperationResult<LoginResponseDto>.FailureResult("User account is not active");

        if (!user.IsEmailConfirmed)
            return OperationResult<LoginResponseDto>.FailureResult("Email has not been confirmed");
        
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result is PasswordVerificationResult.Failed)
            return OperationResult<LoginResponseDto>.FailureResult("Invalid username/email or password");
            
        user.RecordLogin();
        await _userRepository.UpdateAsync(user);
        var tokenResult = _jwtService.GenerateTokenResult(user);
        
        var response = new LoginResponseDto
        {
            AccessToken = tokenResult.AccessToken,
            TokenType = tokenResult.TokenType, 
            ExpiresIn = tokenResult.ExpiresIn
        };
        
        _logger.Information("User successfully authenticated: {Type} == {Id}", dto.IdentifierType, dto.Identifier);
        return OperationResult<LoginResponseDto>.SuccessResult(response);
    }
    
    /// <summary>
        /// Retrieves a user by email or username based on the identifier type in the authentication request.
        /// </summary>
        /// <param name="request">The authentication request containing the identifier and its type.</param>
        /// <returns>The matching user if found; otherwise, null.</returns>
        private async Task<User?> GetUserByEmailOrUsernameAsync(AuthDto request) =>
        request.IdentifierType switch
        {
            AuthIdentifierType.Email    => await _userRepository.GetByEmailAsync(request.Identifier),
            AuthIdentifierType.Username => await _userRepository.GetByUsernameAsync(request.Identifier),
            _ => null
        };
}
