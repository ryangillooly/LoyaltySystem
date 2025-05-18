using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.Application.DTOs.AuthDtos;

public class LinkCustomerDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;
        
    [Required]
    public string CustomerId { get; set; } = string.Empty;
}