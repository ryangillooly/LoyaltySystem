namespace LoyaltySystem.Application.DTOs;

public class UserProfileDto : ProfileDto
{
    public string Status { get; set; } = string.Empty; // Consider using UserStatus enum directly if appropriate
    public string? CustomerId { get; set; } // String representation (prefixed ID)
    public List<string> Roles { get; set; } = new (); // Consider using RoleType enum directly
}