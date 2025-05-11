using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Infrastructure.Entities;
using System.Security.Cryptography;

namespace LoyaltySystem.Application.Services;

public class TokenService : ITokenService
{
    private readonly IPasswordResetTokenRepository _tokenRepository;
    private readonly TimeSpan _tokenLifetime = TimeSpan.FromHours(1);

    public TokenService(IPasswordResetTokenRepository tokenRepository) =>
        _tokenRepository = tokenRepository;

    public async Task<string> GeneratePasswordResetTokenAsync(User user)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var expiry = DateTime.UtcNow.Add(_tokenLifetime);

        await _tokenRepository.SaveAsync
        (
            new PasswordResetToken
            (
                userId: user.Id,
                token: token,
                expiresAt: expiry,
                isUsed: false, 
                null
            )
        );

        return token;
    }

    public async Task<bool> ValidatePasswordResetTokenAsync(User user, string token)
    {
        var record = await _tokenRepository.GetByUserIdAndTokenAsync(user.Id, token);
        
        return 
            record is { IsUsed: false } && 
            record.ExpiresAt >= DateTime.UtcNow;
    }

    public async Task InvalidatePasswordResetTokenAsync(User user, string token)
    {
        var record = await _tokenRepository.GetByUserIdAndTokenAsync(user.Id, token);
        if (record is { })
        {
            record.IsUsed = true;
            await _tokenRepository.UpdateAsync(record);
        }
    }
}