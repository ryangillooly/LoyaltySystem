
using LoyaltySystem.Domain.Entities;

namespace LoyaltySystem.Application.DTOs.AuthDtos;

public class UpdateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
    public bool? IsEmailConfirmed { get; set; } = null;
}