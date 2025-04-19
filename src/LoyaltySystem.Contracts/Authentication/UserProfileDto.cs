using System;
using System.Collections.Generic;
using LoyaltySystem.Domain.Enums; // Assuming UserStatus and RoleType are here

namespace LoyaltySystem.Contracts.Authentication
{
    /// <summary>
    /// Data Transfer Object representing the user's profile information,
    /// potentially combining data from User and Customer entities.
    /// </summary>
    public class UserProfileDto
    {
        /// <summary>
        /// The user-friendly prefixed ID (e.g., usr_xxxxxx).
        /// Renamed from PrefixedId.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The first name of the user (typically from the associated Customer record).
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// The last name of the user (typically from the associated Customer record).
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// The username for login.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// The user's email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The current status of the user account (e.g., Active, Inactive).
        /// </summary>
        public string Status { get; set; } = string.Empty; // Consider using UserStatus enum directly if appropriate

        /// <summary>
        /// The user-friendly prefixed ID of the associated customer, if applicable (e.g., cus_yyyyyy).
        /// </summary>
        public string? CustomerId { get; set; } // String representation (prefixed ID)

        /// <summary>
        /// The date and time the user account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The date and time the user last logged in.
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// List of roles assigned to the user (e.g., ["Customer", "Admin"]).
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>(); // Consider using RoleType enum directly
    }
} 