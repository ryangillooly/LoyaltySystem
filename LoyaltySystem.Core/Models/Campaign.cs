namespace LoyaltySystem.Core.Models;

public class Campaign
{
    public Guid Id { get; set; }    = Guid.NewGuid();
    public Guid BusinessId { get; set; } = Guid.Empty;
    public string Name { get; set; }     = string.Empty;
    public List<Reward> Rewards { get; set; } = new ();
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsActive { get; set; } = true;
    
    public static Campaign Merge(Campaign current, Campaign updated) =>
        new ()
        {
            Id         = current.Id,
            BusinessId = current.BusinessId,
            Name       = string.IsNullOrEmpty(updated.Name) ? current.Name : updated.Name,
            Rewards    = updated.Rewards.Count is 0 ? current.Rewards : updated.Rewards,
            StartTime  = updated.StartTime != current.StartTime && updated.StartTime == DateTime.MinValue ? current.StartTime : updated.StartTime,
            EndTime    = updated.EndTime   != current.EndTime   && updated.EndTime   == DateTime.MinValue ? current.EndTime   : updated.EndTime,
            IsActive   = updated.IsActive  == current.IsActive  ? current.IsActive  : updated.IsActive
        };
}