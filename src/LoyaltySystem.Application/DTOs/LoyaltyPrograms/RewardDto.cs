using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;

namespace LoyaltySystem.Application.DTOs.LoyaltyPrograms;

public class RewardDto
{
    public string Id { get; set; }
    public string ProgramId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int RequiredValue { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
        
    public RewardDto(Reward reward)
    {
        ArgumentNullException.ThrowIfNull(reward);
            
        Id = reward.Id.ToString();
        ProgramId = reward.ProgramId.ToString();
        Title = reward.Title;
        Description = reward.Description;
        RequiredValue = reward.RequiredValue;
        ValidFrom = reward.ValidFrom;
        ValidTo = reward.ValidTo;
        IsActive = reward.IsActive;
        CreatedAt = reward.CreatedAt;
        UpdatedAt = reward.UpdatedAt;
    }
    
    public static Reward ToDomain(RewardDto dto) =>
        new
        (
            LoyaltyProgramId.FromString(dto.ProgramId),
            dto.Title,
            dto.Description,
            dto.RequiredValue,
            dto.ValidFrom,
            dto.ValidTo
        );
}