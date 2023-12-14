namespace LoyaltySystem.Core.Models.Persistance;

public class BusinessUserPermissionsDynamoModel
{
    public string PK { get; set; } = string.Empty;
    public string SK { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    
    public Guid BusinessId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = String.Empty;
    public DateTime Timestamp { get; set; }
}