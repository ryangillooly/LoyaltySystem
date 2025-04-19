using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LoyaltySystem.Application.DTOs
{
    public class LoginRequestDto 
    {
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Response DTO for successful authentication, following standard naming conventions.
    /// </summary>
    public class AuthResponseDto 
    {
        /// <summary>
        /// The JWT access token.
        /// </summary>
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// The type of token (always "Bearer" for this implementation).
        /// </summary>
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = "Bearer"; // Default to Bearer

        /// <summary>
        /// The lifetime of the access token in seconds.
        /// </summary>
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Optional refresh token (not implemented in this version).
        /// </summary>
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; } // Nullable, not currently used
    }
    
    public class RegisterUserDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string LastName { get; set; } = string.Empty;
        
        // [Required]
        [StringLength(100, MinimumLength = 2)]
        public string UserName { get; set; } = string.Empty;
        
        // [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string Phone { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
        
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
    
    public class UpdateProfileDto
    {
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(100, MinimumLength = 2)]
        public string UserName { get; set; } = string.Empty;
        
        [Phone]
        public string Phone { get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
 
        [StringLength(100, MinimumLength = 6)] 
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
    
    public class UserRoleDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = string.Empty;
    }

    public class LinkCustomerDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string CustomerId { get; set; } = string.Empty;
    }
} 