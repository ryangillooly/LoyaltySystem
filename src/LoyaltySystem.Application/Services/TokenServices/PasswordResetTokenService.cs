using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;

namespace LoyaltySystem.Application.Services.TokenServices;

public class PasswordResetTokenService : IPasswordResetTokenService 
{
    private readonly IPasswordResetTokenRepository _tokenRepository;

    public PasswordResetTokenService(IPasswordResetTokenRepository tokenRepository) =>
        _tokenRepository = tokenRepository;
    
    public async Task<string> GenerateTokenAsync(User user)
    {
        var token = SecurityUtils.GenerateSecureToken();
        await _tokenRepository.SaveAsync
        (
            new PasswordResetToken
            (
                userId: user.Id,
                token: token,
                expiresAt: DateTime.UtcNow.AddMinutes(30),
                isUsed: false, 
                null
            )
        );

        return token;
    }
    public async Task<bool> ValidateTokenAsync(User user, string token)
    {
        var record = await _tokenRepository.GetByUserIdAndTokenAsync(user.Id, token);
        
        return 
            record is { IsUsed: false } && 
            record.ExpiresAt >= DateTime.UtcNow;
    }

    public async Task InvalidateTokenAsync(User user, string token)
    {
        var record = await _tokenRepository.GetByUserIdAndTokenAsync(user.Id, token);
        if (record is { })
        {
            record.IsUsed = true;
            await _tokenRepository.UpdateAsync(record);
        }
    }
}
