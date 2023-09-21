using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Models;

public class AuditRecord
{
    public AuditRecord
    (
        EntityType entityType,
        Guid entityId,
        ActionType actionType
    )
    {
        EntityType = entityType;
        EntityId = entityId;
        ActionType = actionType;
    }
    public Guid AuditId { get; set; } = Guid.NewGuid();
    public EntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public ActionType ActionType { get; set; }
    public string ActionDetails { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Guid? UserId { get; set; }
    public string? Source { get; set; } = string.Empty;
}