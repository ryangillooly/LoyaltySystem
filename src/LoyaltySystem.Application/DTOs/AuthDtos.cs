using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Application.DTOs
{
    /// <summary>
    /// Data transfer object for user information.
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        public string? Id { get; set; }
        
        /// <summary>
        /// The username.
        /// </summary>
        public string? Username { get; set; }
        
        /// <summary>
        /// The user's email address.
        /// </summary>
        public string? Email { get; set; }
        
        /// <summary>
        /// The customer ID associated with this user, if any.
        /// </summary>
        public string? CustomerId { get; set; }
        
        /// <summary>
        /// The user's status.
        /// </summary>
        public string? Status { get; set; }
        
        /// <summary>
        /// The roles assigned to this user.
        /// </summary>
        public List<string> Roles { get; set; } = new();
        
        /// <summary>
        /// When the user was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// When the user last logged in.
        /// </summary>
        public DateTime? LastLoginAt { get; set; }
    }
    
    /// <summary>
    /// Request model for user login.
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>
        /// The username or email.
        /// </summary>
        [Required]
        public string? Username { get; set; }
        
        /// <summary>
        /// The password.
        /// </summary>
        [Required]
        public string? Password { get; set; }
    }
    
    /// <summary>
    /// Response model for successful authentication.
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>
        /// The JWT token.
        /// </summary>
        public string? Token { get; set; }
        
        /// <summary>
        /// Information about the authenticated user.
        /// </summary>
        public UserDto? User { get; set; }
    }
    
    /// <summary>
    /// Request model for user registration.
    /// </summary>
    public class RegisterUserDto
    {
        /// <summary>
        /// The desired username.
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string? Username { get; set; }
        
        /// <summary>
        /// The user's email address.
        /// </summary>
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        
        /// <summary>
        /// The password.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; }
        
        /// <summary>
        /// The password confirmation.
        /// </summary>
        [Compare("Password")]
        public string? ConfirmPassword { get; set; }
    }
    
    /// <summary>
    /// Request model for updating a user's profile.
    /// </summary>
    public class UpdateProfileDto
    {
        /// <summary>
        /// The user's email address.
        /// </summary>
        [EmailAddress]
        public string? Email { get; set; }
        
        /// <summary>
        /// The current password (required if changing password).
        /// </summary>
        public string? CurrentPassword { get; set; }
        
        /// <summary>
        /// The new password (optional).
        /// </summary>
        [StringLength(100, MinimumLength = 6)]
        public string? NewPassword { get; set; }
        
        /// <summary>
        /// The new password confirmation.
        /// </summary>
        [Compare("NewPassword")]
        public string? ConfirmNewPassword { get; set; }
    }
    
    /// <summary>
    /// Request model for adding or removing a role from a user.
    /// </summary>
    public class UserRoleDto
    {
        /// <summary>
        /// The user ID.
        /// </summary>
        [Required]
        public string? UserId { get; set; }
        
        /// <summary>
        /// The role to add or remove.
        /// </summary>
        [Required]
        public string? Role { get; set; }
    }
    
    /// <summary>
    /// Request model for linking a customer to a user.
    /// </summary>
    public class LinkCustomerDto
    {
        /// <summary>
        /// The user ID.
        /// </summary>
        [Required]
        public string? UserId { get; set; }
        
        /// <summary>
        /// The customer ID.
        /// </summary>
        [Required]
        public string? CustomerId { get; set; }
    }
} 