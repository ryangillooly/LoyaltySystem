using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.Interfaces.Auth;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using Serilog;

namespace LoyaltySystem.Application.Services.Auth;

public class EmailVerificationService : IEmailVerificationService 
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailVerificationRepository _tokenRepository;
    private readonly ILogger _logger;

    public EmailVerificationService
    (
        IUserRepository userRepository,
        IEmailVerificationRepository emailTokenRepository,
        ILogger logger
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _tokenRepository = emailTokenRepository ?? throw new ArgumentNullException(nameof(emailTokenRepository));
    }

    public async Task<string> GenerateTokenAsync(User user)
    {
        var token = SecurityUtils.GenerateSecureToken();
        await _tokenRepository.SaveAsync
        (
            new EmailConfirmationToken
            (
                userId: user.Id,
                token: token,
                expiresAt: DateTime.UtcNow.AddDays(1),
                isUsed: false, 
                null
            )
        );

        return token;
    }
    public async Task<bool> ValidateTokenAsync(User user, string token)
    {
        var record = await _tokenRepository.GetByTokenAsync(token);
        
        return 
            record is { IsUsed: false } && 
            record.ExpiresAt >= DateTime.UtcNow;
    }
    
    public async Task InvalidateTokenAsync(User user, string token)
    {
        var record = await _tokenRepository.GetByTokenAsync(token);
        if (record is { })
        {
            record.IsUsed = true;
            await _tokenRepository.UpdateAsync(record);
        }
    }
    
    public async Task<OperationResult> VerifyEmailAsync(string token)
    {
        ArgumentNullException.ThrowIfNull(nameof(token));

        var user = await _userRepository.GetByEmailConfirmationTokenAsync(token);
        if (user == null)
            return OperationResult.FailureResult("Invalid or expired token.");

        if (user.IsEmailConfirmed)
        {
            _logger.Information("User {UserId} email already verified", user.Id);
            return OperationResult.SuccessResult();
        }

        var emailToken = await _tokenRepository.GetByTokenAsync(token);
        if (emailToken is null)
            return OperationResult.FailureResult("Email confirmation token could not be found");

        var validation = ValidateTokenAsync(user, token);
        if (!validation.Result)
            return OperationResult.FailureResult("Invalid or expired token.");

        user.IsEmailConfirmed = true;
        await _userRepository.UpdateAsync(user);

        emailToken.IsUsed = true;
        await _tokenRepository.UpdateAsync(emailToken);

        _logger.Information("User {UserId} verified their email.", user.Id);

        return OperationResult.SuccessResult();
    }
}
