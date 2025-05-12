using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.PasswordReset;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Application.Interfaces.Auth;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace LoyaltySystem.Application.Services.Auth;

public class PasswordResetService : IPasswordResetService 
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenService _passwordResetTokenService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public PasswordResetService(
        IUserRepository userRepository,
        IPasswordResetTokenService passwordTokenService,
        IEmailService emailService,
        IPasswordHasher<User> passwordHasher,
        ILogger logger
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _passwordResetTokenService = passwordTokenService ?? throw new ArgumentNullException(nameof(passwordTokenService));
    }
    
    public async Task<OperationResult> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        User? user = await GetUserByEmailOrUsernameAsync(request);

        if (user is null)
        {
            _logger.Information("User not found. {Type} == {Identifier}", request.IdentifierType, request.Identifier);
            return OperationResult.FailureResult($"User not found with {request.IdentifierType} == {request.Identifier}");
        }

        var token = await _passwordResetTokenService.GenerateTokenAsync(user);
        await _emailService.SendPasswordResetEmailAsync(user.Email, token);

        _logger.Information("Password successfully reset for {Type} == {Identifier}", request.IdentifierType, request.Identifier);
        return OperationResult.SuccessResult();
    }
    
    public async Task<OperationResult> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        User? user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null)
        {
            _logger.Information("User not found. {Email}", request.Email);
            return OperationResult.FailureResult($"User not found with the Email == {request.Email}");
        }

        var isValid = await _passwordResetTokenService.ValidateTokenAsync(user, request.Token);
        if (!isValid)
        {
            _logger.Information("Password reset token is invalid. {Token}. For Email {Email}", request.Token, request.Email);
            return OperationResult.FailureResult("Invalid or expired reset token.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        await _userRepository.UpdateAsync(user);

        await _passwordResetTokenService.InvalidateTokenAsync(user, request.Token);
        
        _logger.Information("Successfully reset password For Email {Email}", request.Email);
        return OperationResult.SuccessResult();
    }
    
    private async Task<User?> GetUserByEmailOrUsernameAsync(AuthDto request) =>
        request.IdentifierType switch
        {
            AuthIdentifierType.Email    => await _userRepository.GetByEmailAsync(request.Identifier),
            AuthIdentifierType.Username => await _userRepository.GetByUsernameAsync(request.Identifier),
            _ => null
        };
}
