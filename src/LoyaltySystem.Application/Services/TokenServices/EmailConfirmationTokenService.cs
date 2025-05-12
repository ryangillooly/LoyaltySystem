using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;

namespace LoyaltySystem.Application.Services.TokenServices;

public class EmailConfirmationTokenService : IEmailConfirmationTokenService 
{
    private readonly IEmailConfirmationTokenRepository _tokenRepository;

    public EmailConfirmationTokenService(IEmailConfirmationTokenRepository tokenRepository) =>
        _tokenRepository = tokenRepository;
    
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
}
