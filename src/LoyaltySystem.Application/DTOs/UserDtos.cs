using LoyaltySystem.Domain.Enums;
using System;
using System.Collections.Generic;

namespace LoyaltySystem.Application.DTOs;

// Modify the existing UserDto definition
public class UserDto
{
    /// <summary>
    /// The unique identifier (GUID) of the user.
    /// </summary>
    public Guid Id { get; set; } // Changed type from string to Guid

    /// <summary>
    /// The human-readable prefixed ID (e.g., usr_xxxx).
    /// </summary>
    public string PrefixedId { get; set; } = string.Empty; // Added this property

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty; 
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Roles { get; set; } = new();
}

// Other DTO definitions might follow...