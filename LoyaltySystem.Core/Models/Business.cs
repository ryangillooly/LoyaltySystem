using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Models;

public class Business
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public Guid ManagerId { get; set; }
    public string BusinessName { get; set; }
    public string Description { get; set; }
    public Location Location { get; set; }
    public ContactInfo ContactInfo { get; set; }
    public OpeningHours OpeningHours { get; set; }
    public BusinessStatus Status { get; set; }
    
}