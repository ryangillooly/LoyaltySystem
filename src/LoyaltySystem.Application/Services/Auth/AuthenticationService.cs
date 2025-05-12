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
    
    public async Task<OperationResult<AuthResponseDto>> AuthenticateAsync(LoginRequestDto dto)
    {
        User? user = await GetUserByEmailOrUsernameAsync(dto);
            
        if (user is null)
            return OperationResult<AuthResponseDto>.FailureResult("Invalid username/email or password");
                
        if (user.Status != UserStatus.Active)
            return OperationResult<AuthResponseDto>.FailureResult("User account is not active");

        if (!user.IsEmailConfirmed)
            return OperationResult<AuthResponseDto>.FailureResult("Email has not been confirmed");
        
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
            return OperationResult<AuthResponseDto>.FailureResult("Invalid username/email or password");
            
        user.RecordLogin();
        _logger.Information("User successfully authenticated: {Type} == {Identifier}", dto.IdentifierType, dto.Identifier);
        await _userRepository.UpdateAsync(user);
        
        var tokenResult = _jwtService.GenerateTokenResult(user);
        
        var response = new AuthResponseDto
        {
            AccessToken = tokenResult.AccessToken,
            TokenType = tokenResult.TokenType, 
            ExpiresIn = tokenResult.ExpiresIn,
            // RefreshToken = tokenResult.RefreshToken // Assign if/when implemented
        };
        
        return OperationResult<AuthResponseDto>.SuccessResult(response);
    }
    
    private async Task<User?> GetUserByEmailOrUsernameAsync(AuthDto request) =>
        request.IdentifierType switch
        {
            AuthIdentifierType.Email    => await _userRepository.GetByEmailAsync(request.Identifier),
            AuthIdentifierType.Username => await _userRepository.GetByUsernameAsync(request.Identifier),
            _ => null
        };
}
