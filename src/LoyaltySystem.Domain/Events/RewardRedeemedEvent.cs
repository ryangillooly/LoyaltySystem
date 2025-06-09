using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Domain.Events;

/// <summary>
/// Domain event raised when a reward is redeemed from a loyalty card
/// Enhanced with full business context and transaction details
/// </summary>
public sealed record RewardRedeemedEvent : DomainEventBase
{
    public override Guid AggregateId { get; init; }
    public override string AggregateType { get; init; } = "LoyaltyCard";
    
    // Core Transaction Data
    public Guid CardId { get; init; }
    public Guid CustomerId { get; init; }
    public Guid ProgramId { get; init; }
    public Guid RewardId { get; init; }
    public Guid StoreId { get; init; }
    public Guid? StaffId { get; init; }
    
    // Reward Details
    public string RewardTitle { get; init; }
    public string RewardDescription { get; init; }
    public int RequiredValue { get; init; }
    public string RewardType { get; init; } // "Stamps" or "Points"
    
    // Balance Changes
    public int StampsBefore { get; init; }
    public int StampsAfter { get; init; }
    public decimal PointsBefore { get; init; }
    public decimal PointsAfter { get; init; }
    public decimal CurrencyValue { get; init; }
    
    // Business Context
    public string ProgramName { get; init; }
    public string StoreName { get; init; }
    public string? StaffName { get; init; }
    
    // Fraud & Validation Context
    public bool FraudCheckPassed { get; init; }
    public bool DailyLimitCheckPassed { get; init; }
    public int DailyRedemptionsBeforeTransaction { get; init; }
    public int DailyRedemptionsAfterTransaction { get; init; }
    public decimal DailyCurrencyValueBefore { get; init; }
    public decimal DailyCurrencyValueAfter { get; init; }
    
    // Remaining Balance Context
    public bool HasRemainingBalance { get; init; }
    public int RemainingStamps { get; init; }
    public decimal RemainingPoints { get; init; }
    public bool CanRedeemMore { get; init; }
    public int? NextRewardThreshold { get; init; }
    public int ValueToNextReward { get; init; }
    
    // Tier Information (for points-based programs)
    public string? CurrentTierName { get; init; }
    public bool TierDowngraded { get; init; }
    public string? NewTierName { get; init; }
    
    public RewardRedeemedEvent(
        Guid cardId,
        Guid customerId,
        Guid programId,
        Guid rewardId,
        Guid storeId,
        string rewardTitle,
        string rewardDescription,
        int requiredValue,
        string rewardType,
        int stampsBefore,
        decimal pointsBefore,
        string programName,
        string storeName,
        Guid? staffId = null,
        string? staffName = null,
        bool fraudCheckPassed = true,
        bool dailyLimitCheckPassed = true,
        int dailyRedemptionsBeforeTransaction = 0,
        decimal dailyCurrencyValueBefore = 0,
        decimal currencyValue = 0,
        string? currentTierName = null,
        string? newTierName = null,
        int? nextRewardThreshold = null)
    {
        AggregateId = cardId;
        CardId = cardId;
        CustomerId = customerId;
        ProgramId = programId;
        RewardId = rewardId;
        StoreId = storeId;
        StaffId = staffId;
        
        RewardTitle = rewardTitle;
        RewardDescription = rewardDescription;
        RequiredValue = requiredValue;
        RewardType = rewardType;
        
        StampsBefore = stampsBefore;
        StampsAfter = rewardType == "Stamps" ? stampsBefore - requiredValue : stampsBefore;
        PointsBefore = pointsBefore;
        PointsAfter = rewardType == "Points" ? pointsBefore - requiredValue : pointsBefore;
        CurrencyValue = currencyValue;
        
        ProgramName = programName;
        StoreName = storeName;
        StaffName = staffName;
        
        FraudCheckPassed = fraudCheckPassed;
        DailyLimitCheckPassed = dailyLimitCheckPassed;
        DailyRedemptionsBeforeTransaction = dailyRedemptionsBeforeTransaction;
        DailyRedemptionsAfterTransaction = dailyRedemptionsBeforeTransaction + 1;
        DailyCurrencyValueBefore = dailyCurrencyValueBefore;
        DailyCurrencyValueAfter = dailyCurrencyValueBefore + currencyValue;
        
        RemainingStamps = StampsAfter;
        RemainingPoints = PointsAfter;
        HasRemainingBalance = (rewardType == "Stamps" && StampsAfter > 0) || (rewardType == "Points" && PointsAfter > 0);
        
        NextRewardThreshold = nextRewardThreshold;
        ValueToNextReward = nextRewardThreshold.HasValue 
            ? Math.Max(0, nextRewardThreshold.Value - (rewardType == "Stamps" ? StampsAfter : (int)PointsAfter))
            : 0;
        CanRedeemMore = HasRemainingBalance && (NextRewardThreshold == null || 
            (rewardType == "Stamps" ? StampsAfter : (int)PointsAfter) >= NextRewardThreshold);
        
        CurrentTierName = currentTierName;
        NewTierName = newTierName;
        TierDowngraded = !string.IsNullOrEmpty(currentTierName) && 
                        !string.IsNullOrEmpty(newTierName) && 
                        newTierName != currentTierName;
    }
} 