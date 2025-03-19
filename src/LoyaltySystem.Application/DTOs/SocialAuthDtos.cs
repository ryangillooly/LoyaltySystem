using System.ComponentModel.DataAnnotations;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.DTOs;

public class SocialAuthRequestDto
{
    [Required]
    public string AuthCode { get; set; }
    
    public string State { get; set; }
    
    public string Nonce { get; set; }
    
    [Required]
    public SocialLoginProvider Provider { get; set; }
}

public class SocialAuthResponseDto
{
    public string Token { get; set; }
    public UserDto User { get; set; }
    public bool IsNewUser { get; set; }
    public string SocialId { get; set; }
    public string SocialEmail { get; set; }
}

public class SocialLinkRequestDto
{
    [Required]
    public string AuthCode { get; set; }
    
    public string Nonce { get; set; }
    
    [Required]
    public SocialLoginProvider Provider { get; set; }
}

public class SocialUnlinkRequestDto
{
    [Required]
    public SocialLoginProvider Provider { get; set; }
} 