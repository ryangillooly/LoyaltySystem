using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.DTOs.LoyaltyPrograms;

public class LoyaltyProgramDto
{
    public string Id { get; set; }
    public string BrandId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public LoyaltyProgramType Type { get; set; }
    public ExpirationPolicyDto ExpirationPolicy { get; set; }
    public int? StampThreshold { get; set; }
    public decimal? PointsConversionRate { get; set; }
    public PointsConfigDto PointsConfig { get; set; }
    public int? DailyStampLimit { get; set; }
    public decimal? MinimumTransactionAmount { get; set; }
    public bool HasTiers { get; set; }
    public List<TierDto> Tiers { get; set; } = new ();
    public string TermsAndConditions { get; set; }
    public int EnrollmentBonusPoints { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public BrandDto Brand { get; set; }
    public List<RewardDto> Rewards { get; set; } = new List<RewardDto>();
}