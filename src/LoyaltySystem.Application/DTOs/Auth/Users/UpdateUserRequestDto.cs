using LoyaltySystem.Domain.Entities;

namespace LoyaltySystem.Application.DTOs.AuthDtos;

public class UpdateUserRequestDto 
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
    public bool? IsEmailConfirmed { get; set; } = null;

    /// <summary>
        /// Creates an <see cref="UpdateUserRequestDto"/> instance from an <see cref="InternalUserDto"/>, mapping email, username, phone, and setting the current password to the customer ID.
        /// </summary>
        /// <param name="internalUserDto">The source user data to map from.</param>
        /// <returns>A new <see cref="UpdateUserRequestDto"/> populated with values from the provided user DTO.</returns>
        public static UpdateUserRequestDto FromUserDto(InternalUserDto internalUserDto) =>
        new()
        {
            Email = internalUserDto.Email,
            Username = internalUserDto.Username,
            Phone = internalUserDto.Phone,
            CurrentPassword = internalUserDto.CustomerId
        };
}