using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Domain.Entities;

public class Reward : Entity<RewardId>
{
    public LoyaltyProgramId ProgramId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int RequiredValue { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
    
    public Reward
    (
        LoyaltyProgramId programId,
        string title,
        string description,
        int requiredValue,
        DateTime? validFrom = null,
        DateTime? validTo = null
    )
    : base(new RewardId())
    {
        if (requiredValue <= 0)
            throw new ArgumentException("Requiredvalue must be greater than zero", nameof(requiredValue));
        
        if (validFrom.HasValue && validTo.HasValue && 
            validFrom.Value > validTo.Value)
            throw new ArgumentException("Valid from date must be before valid to date");
        
        ProgramId = programId;
        Title = title;
        Description = description;
        RequiredValue = requiredValue;
        ValidFrom = validFrom;
        ValidTo = validTo;
        IsActive = true;
    }
    
    public void Update
    (
        string title,
        string description,
        int requiredValue,
        DateTime? validFrom = null,
        DateTime? validTo = null
    )
    {
        if (requiredValue <= 0)
            throw new ArgumentException("Required value must be greater than zero", nameof(requiredValue));
        
        if (validFrom.HasValue && validTo.HasValue && validFrom.Value > validTo.Value)
            throw new ArgumentException("Valid from date must be before valid to date");

        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        RequiredValue = requiredValue;
        ValidFrom = validFrom;
        ValidTo = validTo;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsValidAt(DateTime date)
    {
        if (!IsActive)
            return false;
        
        if (ValidFrom.HasValue && date < ValidFrom.Value)
            return false;

        return !ValidTo.HasValue || date <= ValidTo.Value;

    }
}