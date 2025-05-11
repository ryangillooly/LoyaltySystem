using LoyaltySystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.Application.DTOs.Auth.Social;

public class SocialAuthRequestDto
{
    public string AuthCode { get; set; }
    public string State { get; set; }
    public string Nonce { get; set; }
    public SocialLoginProvider Provider { get; set; }
}