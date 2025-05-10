using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.Application.DTOs.AuthDtos;

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