using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Models;

public class Business
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerId { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Location Location { get; set; } = new ();
    public ContactInfo ContactInfo { get; set; } = new ();
    public OpeningHours OpeningHours { get; set; } = new ();
    public BusinessStatus Status { get; set; } = BusinessStatus.Active;

}