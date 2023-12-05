namespace LoyaltySystem.Core.Models.Persistance;

public class BusinessDynamoModel
{
    public string PK { get; set; } = string.Empty;
    public string SK { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Location Location { get; set; } = new ();
    public string? PhoneNumber { get; set; }
    public string Email { get; set; } = string.Empty;
    public OpeningHours OpeningHours { get; set; } = new ();
    public string Status { get; set; } = string.Empty;
}

