using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LoyaltySystem.Domain.Common;
using System.ComponentModel;

namespace LoyaltySystem.Application.DTOs
{
    public class UserDto 
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
    
    public class LoginRequestDto 
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
    
    public class AuthResponseDto 
    {
        public string Token { get; set; } = string.Empty;
        public UserDto? User { get; set; }
    }
    
    public class RegisterUserDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
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