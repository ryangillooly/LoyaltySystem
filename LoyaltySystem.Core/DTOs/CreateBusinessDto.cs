using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.DTOs;

public class CreateBusinessDto
{
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Location Location { get; set; } = new ();
    public ContactInfo ContactInfo { get; set; } = new ();
    public OpeningHours OpeningHours { get; set; } = new ();
}