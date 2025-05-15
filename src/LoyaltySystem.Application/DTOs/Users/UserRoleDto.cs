using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.Application.DTOs.AuthDtos;

public class UserRoleDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;
        
    [Required]
    public string Role { get; set; } = string.Empty;
}