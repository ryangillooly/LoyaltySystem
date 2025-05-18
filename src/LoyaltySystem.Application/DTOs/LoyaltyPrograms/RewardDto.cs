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
        
    /// <summary>
    /// Initializes a new instance of the <see cref="RewardDto"/> class by mapping properties from a <see cref="Reward"/> domain entity.
    /// </summary>
    /// <param name="reward">The <see cref="Reward"/> domain entity to map from. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="reward"/> is null.</exception>
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
    
    /// <summary>
        /// Converts a <see cref="RewardDto"/> instance to a corresponding <see cref="Reward"/> domain entity.
        /// </summary>
        /// <param name="dto">The data transfer object containing reward information.</param>
        /// <returns>A new <see cref="Reward"/> domain entity with properties mapped from the DTO.</returns>
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