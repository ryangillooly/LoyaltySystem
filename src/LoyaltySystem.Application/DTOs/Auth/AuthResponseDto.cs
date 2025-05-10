using System.Text.Json.Serialization;

namespace LoyaltySystem.Application.DTOs.AuthDtos;

public class AuthResponseDto 
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "Bearer"; // Default to Bearer
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; } // Nullable, not currently used
}
