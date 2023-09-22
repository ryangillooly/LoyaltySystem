namespace LoyaltySystem.Core.Models;

public class Campaign
{
    public Guid Id { get; set; }         = Guid.NewGuid();
    public Guid BusinessId { get; set; } = Guid.Empty;
    public string Name { get; set; }     = string.Empty;
    public string Rewards { get; set; }  = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsActive { get; set; } = true;
    

    /*


StartTime: Start time of the campaign (Epoch timestamp for easy comparison).
EndTime: End time of the campaign.
IsActive: A boolean indicating if the campaign is currently active.
Criteria: Additional conditions if any.
     */
}